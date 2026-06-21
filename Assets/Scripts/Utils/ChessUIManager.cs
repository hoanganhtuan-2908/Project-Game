using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChessUIManager : MonoBehaviour, IChessUIManager
{
    [Header("Screens")]
    [SerializeField] private GameObject GameOverScreen;
    [SerializeField] private GameObject PauseMenuScreen;
    [SerializeField] private GameObject GameModeSelectionScreen;

    [Header("Texts")]
    [SerializeField] private TMP_Text finishText;

    [Header("Difficulty / Level Selection")]
    [SerializeField] private TMP_Dropdown gameLevelSelection;

    [Header("Skins")]
    [SerializeField] private TMP_Dropdown skinSelectionDropdown;
    [SerializeField] private List<ChessSkin> availableSkins;

    private ChessSkin selectedSkin;
    public ChessSkin SelectedSkin => selectedSkin;
    public List<ChessSkin> AvailableSkins => availableSkins;

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

    private void Start()
    {
        // Tự động kích hoạt Multiplayer nếu có cờ báo từ MainMenu (nếu scene cũ cần tương thích)
        if (PlayerPrefs.GetInt("AutoStartMultiplayer", 0) == 1)
        {
            PlayerPrefs.SetInt("AutoStartMultiplayer", 0);
            PlayerPrefs.Save();
            OnMultiPlayerModeSelected();
        }
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

    public void OnGameLaunched()
    {
        if (GameOverScreen != null) GameOverScreen.SetActive(false);
        if (GameModeSelectionScreen != null) GameModeSelectionScreen.SetActive(true);
    }

    public void OnSinglePlayerModeSelected()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("SinglePlayer");
    }

    public void OnMultiPlayerModeSelected()
    {
        // Chuyển sang scene Multiplayer Lobbies
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MultiplayerPlayerLobbySence");
    }

    public void OnGameStarted()
    {
        if (GameOverScreen != null) GameOverScreen.SetActive(false);
        if (GameModeSelectionScreen != null) GameModeSelectionScreen.SetActive(false);
        if (PauseMenuScreen != null) PauseMenuScreen.SetActive(false);

        // Bắt đầu phát nhạc trận đấu
        if (FMODAudioManager.Instance != null)
        {
            FMODAudioManager.Instance.PlayGameplayBGM();
        }
    }

    public void OnGameFinished(string winner)
    {
        if (GameOverScreen != null) GameOverScreen.SetActive(true);
        if (finishText != null) finishText.text = string.Format("{0} won", winner);

        // Phát nhạc thắng/thua
        if (FMODAudioManager.Instance != null)
        {
            bool localPlayerWon = false;
            if (activeController != null)
            {
                localPlayerWon = activeController.IsLocalPlayerWinner(winner);
            }
            FMODAudioManager.Instance.PlayGameFinishedTheme(localPlayerWon);
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

        // Quay lại nhạc Menu chính
        if (FMODAudioManager.Instance != null)
        {
            FMODAudioManager.Instance.PlayMenuTheme();
        }

        // Quay lại MainMenu Scene
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainMenu")
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        else
        {
            if (activeController != null)
            {
                Board board = FindAnyObjectByType<Board>();
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