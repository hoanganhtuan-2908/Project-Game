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
    public ChessLevel SelectedLevel => (ChessLevel)gameLevelSelection.value;

    private ChessGameController activeController;

    private void Awake()
    {
        gameLevelSelection.AddOptions(Enum.GetNames(typeof(ChessLevel)).ToList());
        OnGameLaunched();
    }

    private void Start()
    {
        InitializeSkinSelection();
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



    internal void OnGameLaunched()
    {
        GameOverScreen.SetActive(false);
        TeamSelectionScreen.SetActive(false);
        ConnectScreen.SetActive(false);
        GameModeSelectionScreen.SetActive(true);
    }

    public void OnSinglePlayerModeSelected()
    {
        GameOverScreen.SetActive(false);
        TeamSelectionScreen.SetActive(false);
        ConnectScreen.SetActive(false);
        GameModeSelectionScreen.SetActive(false);
    }

    public void OnMultiPlayerModeSelected()
    {
        connectionStatus.gameObject.SetActive(true);
        GameOverScreen.SetActive(false);
        TeamSelectionScreen.SetActive(false);
        ConnectScreen.SetActive(true);
        GameModeSelectionScreen.SetActive(false);
    }

    internal void OnGameFinished(string winner)
    {

        GameOverScreen.SetActive(true);
        TeamSelectionScreen.SetActive(false);
        ConnectScreen.SetActive(false);
        finishText.text = string.Format("{0} won", winner);
    }

    public void OnConnect()
    {
        networkManager.SetPlayerLevel((ChessLevel)gameLevelSelection.value);
        networkManager.Connect();
    }

    public void SetConnectionStatusText(string status)
    {
        connectionStatus.text = status;
    }

    internal void ShowTeamSelectionScreen()
    {
        GameOverScreen.SetActive(false);
        TeamSelectionScreen.SetActive(true);
        ConnectScreen.SetActive(false);
    }

    public void OnGameStarted()
    {
        GameOverScreen.SetActive(false);
        TeamSelectionScreen.SetActive(false);
        ConnectScreen.SetActive(false);
        connectionStatus.gameObject.SetActive(false);
        GameModeSelectionScreen.SetActive(false);
        if (PauseMenuScreen != null)
        {
            PauseMenuScreen.SetActive(false);
        }
    }

    public void SelectTeam(int team)
    {
        networkManager.SetPlayerTeam(team);
    }

    internal void RestrictTeamChoice(TeamColor occpiedTeam)
    {
        Button buttonToDeactivate = occpiedTeam == TeamColor.White ? whiteTeamButtonButton : blackTeamButtonButton;
        buttonToDeactivate.interactable = false;
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