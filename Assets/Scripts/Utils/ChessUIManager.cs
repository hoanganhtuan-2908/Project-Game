using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ChessUIManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private NetworkManager networkManager;

    [Header("Buttons")]
    [SerializeField] private Button whiteTeamButtonButton;
    [SerializeField] private Button blackTeamButtonButton;

    [Header("Texts")]
    [SerializeField] private TMP_Text finishText;
    [SerializeField] private TMP_Text connectionStatus;

    [Header("Screen Gameobjects")]
    [SerializeField] private GameObject GameOverScreen;
    [SerializeField] private GameObject ConnectScreen;
    [SerializeField] private GameObject TeamSelectionScreen;
    [SerializeField] private GameObject GameModeSelectionScreen;
    [SerializeField] private GameObject PauseMenuScreen;

    [Header("Other UI")]
    [SerializeField] private TMP_Dropdown gameLevelSelection;

    [Header("Skins/Models UI")]
    [SerializeField] private TMP_Dropdown skinSelectionDropdown;
    [SerializeField] private List<ChessSkin> availableSkins;

    private ChessSkin selectedSkin;
    public ChessSkin SelectedSkin => selectedSkin;
    public ChessLevel SelectedLevel
    {
        get
        {
            if (gameLevelSelection != null)
            {
                return (ChessLevel)gameLevelSelection.value;
            }
            return (ChessLevel)PlayerPrefs.GetInt("SelectedLevel", (int)ChessLevel.Regular);
        }
    }

    private ChessGameController activeController;

    private void Awake()
    {
        if (gameLevelSelection != null)
        {
            gameLevelSelection.ClearOptions();
            gameLevelSelection.AddOptions(Enum.GetNames(typeof(ChessLevel)).ToList());
            
            // Load saved level preference
            int savedLevel = PlayerPrefs.GetInt("SelectedLevel", (int)ChessLevel.Regular);
            gameLevelSelection.value = savedLevel;
            gameLevelSelection.RefreshShownValue();
            
            gameLevelSelection.onValueChanged.AddListener(OnLevelChanged);
        }
        InitializeSkinSelection();
        OnGameLaunched();
    }

    private void InitializeSkinSelection()
    {
        if (skinSelectionDropdown != null && availableSkins != null && availableSkins.Count > 0)
        {
            skinSelectionDropdown.ClearOptions();
            List<string> options = availableSkins.Select(s => string.IsNullOrEmpty(s.skinName) ? s.name : s.skinName).ToList();
            skinSelectionDropdown.AddOptions(options);

            // Load saved skin preference
            int savedIndex = PlayerPrefs.GetInt("SelectedSkinIndex", 0);
            if (savedIndex >= availableSkins.Count || savedIndex < 0) 
                savedIndex = 0;
            
            skinSelectionDropdown.value = savedIndex;
            skinSelectionDropdown.RefreshShownValue();
            selectedSkin = availableSkins[savedIndex];
            
            skinSelectionDropdown.onValueChanged.AddListener(OnSkinChanged);
        }
    }

    private void OnSkinChanged(int index)
    {
        if (availableSkins != null && index >= 0 && index < availableSkins.Count)
        {
            selectedSkin = availableSkins[index];
            PlayerPrefs.SetInt("SelectedSkinIndex", index);
            PlayerPrefs.Save();
            Debug.Log($"Skin changed to: {selectedSkin.skinName}");
        }
    }

    private void OnLevelChanged(int value)
    {
        PlayerPrefs.SetInt("SelectedLevel", value);
        PlayerPrefs.Save();
        Debug.Log($"Level changed to: {(ChessLevel)value}");
    }

    internal void OnGameLaunched()
    {
        if (GameOverScreen != null) GameOverScreen.SetActive(false);
        if (TeamSelectionScreen != null) TeamSelectionScreen.SetActive(false);
        if (ConnectScreen != null) ConnectScreen.SetActive(false);
        if (GameModeSelectionScreen != null) GameModeSelectionScreen.SetActive(true);
    }

    public void OnSinglePlayerModeSelected()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("SinglePlayer");
    }

    public void OnMultiPlayerModeSelected()
    {
        if (connectionStatus != null) connectionStatus.gameObject.SetActive(true);
        if (GameOverScreen != null) GameOverScreen.SetActive(false);
        if (TeamSelectionScreen != null) TeamSelectionScreen.SetActive(false);
        if (ConnectScreen != null) ConnectScreen.SetActive(true);
        if (GameModeSelectionScreen != null) GameModeSelectionScreen.SetActive(false);
    }

    internal void OnGameFinished(string winner)
    {
        if (GameOverScreen != null) GameOverScreen.SetActive(true);
        if (TeamSelectionScreen != null) TeamSelectionScreen.SetActive(false);
        if (ConnectScreen != null) ConnectScreen.SetActive(false);
        if (finishText != null) finishText.text = string.Format("{0} won", winner);
    }

    public void OnConnect()
    {
        if (networkManager != null)
        {
            networkManager.SetPlayerLevel(SelectedLevel);
            networkManager.Connect();
        }
        else
        {
            Debug.LogError("NetworkManager is null. Cannot connect.");
        }
    }

    public void SetConnectionStatusText(string status)
    {
        if (connectionStatus != null)
        {
            connectionStatus.text = status;
        }
    }

    internal void ShowTeamSelectionScreen()
    {
        if (GameOverScreen != null) GameOverScreen.SetActive(false);
        if (TeamSelectionScreen != null) TeamSelectionScreen.SetActive(true);
        if (ConnectScreen != null) ConnectScreen.SetActive(false);
    }

    public void OnGameStarted()
    {
        if (GameOverScreen != null) GameOverScreen.SetActive(false);
        if (TeamSelectionScreen != null) TeamSelectionScreen.SetActive(false);
        if (ConnectScreen != null) ConnectScreen.SetActive(false);
        if (connectionStatus != null) connectionStatus.gameObject.SetActive(false);
        if (GameModeSelectionScreen != null) GameModeSelectionScreen.SetActive(false);
        if (PauseMenuScreen != null)
        {
            PauseMenuScreen.SetActive(false);
        }
    }

    public void SelectTeam(int team)
    {
        if (networkManager != null)
        {
            networkManager.SetPlayerTeam(team);
        }
        else
        {
            Debug.LogError("NetworkManager is null. Cannot select team.");
        }
    }

    internal void RestrictTeamChoice(TeamColor occpiedTeam)
    {
        Button buttonToDeactivate = occpiedTeam == TeamColor.White ? whiteTeamButtonButton : blackTeamButtonButton;
        if (buttonToDeactivate != null)
        {
            buttonToDeactivate.interactable = false;
        }
    }

    public void SetActiveController(ChessGameController controller)
    {
        activeController = controller;
    }

    public void TogglePauseMenu(bool isPaused)
    {
        if (PauseMenuScreen != null)
        {
            PauseMenuScreen.SetActive(isPaused);
        }
    }

    public void OnResumeClicked()
    {
        if (activeController != null)
        {
            activeController.ResumeGame();
        }
    }

    public void OnRestartClicked()
    {
        if (activeController != null)
        {
            activeController.ResumeGame();
            activeController.RestartGame();
        }
    }

    public void OnQuitClicked()
    {
        Time.timeScale = 1f;
        TogglePauseMenu(false);

        // If we are in a gameplay scene (not MainMenu), load MainMenu scene
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainMenu")
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        else
        {
            // Fallback for single-scene setup: clean up and show the mode selector UI
            if (activeController != null)
            {
                Board board = FindObjectOfType<Board>();
                if (board != null)
                {
                    Destroy(board.gameObject);
                }

                Destroy(activeController.gameObject);
                activeController = null;
            }

            OnGameLaunched();
        }
    }
}