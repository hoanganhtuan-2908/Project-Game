using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class MultiplayerUIManager : MonoBehaviour, IChessUIManager
{
    [Header("Screen GameObjects")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject victoryBackground;
    [SerializeField] private GameObject defeatBackground;

    [Header("Buttons")]
    [SerializeField] private Button giveUpButton;
    [SerializeField] private Button backToMainMenuButton;

    [Header("Texts")]
    [SerializeField] private TMP_Text finishText;

    private MultiplayerChessGameController activeController;

    private void Awake()
    {
        // Hide screens by default
        if (gameOverScreen != null) gameOverScreen.SetActive(false);
        if (victoryBackground != null) victoryBackground.SetActive(false);
        if (defeatBackground != null) defeatBackground.SetActive(false);
        if (giveUpButton != null) giveUpButton.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (giveUpButton != null)
        {
            giveUpButton.onClick.AddListener(OnGiveUpClicked);
        }
        if (backToMainMenuButton != null)
        {
            backToMainMenuButton.onClick.AddListener(OnBackToMainMenuClicked);
        }
    }

    public void SetActiveController(MultiplayerChessGameController controller)
    {
        activeController = controller;
    }

    public void OnGameStarted()
    {
        if (gameOverScreen != null) gameOverScreen.SetActive(false);
        if (victoryBackground != null) victoryBackground.SetActive(false);
        if (defeatBackground != null) defeatBackground.SetActive(false);
        if (giveUpButton != null) giveUpButton.gameObject.SetActive(true);

        // Play gameplay BGM
        if (FMODAudioManager.Instance != null)
        {
            FMODAudioManager.Instance.PlayGameplayBGM();
        }
    }

    public void OnGameFinished(string winner)
    {
        if (gameOverScreen != null) gameOverScreen.SetActive(true);
        if (finishText != null) finishText.text = $"{winner} won";
        if (giveUpButton != null) giveUpButton.gameObject.SetActive(false);

        // Determine if local player won or lost
        bool localPlayerWon = false;
        if (activeController != null)
        {
            localPlayerWon = activeController.IsLocalPlayerWinner(winner);
        }

        // Toggle win/lose backgrounds
        if (victoryBackground != null) victoryBackground.SetActive(localPlayerWon);
        if (defeatBackground != null) defeatBackground.SetActive(!localPlayerWon);

        // Play victory/defeat BGM
        if (FMODAudioManager.Instance != null)
        {
            FMODAudioManager.Instance.PlayGameFinishedTheme(localPlayerWon);
        }
    }

    public void TogglePauseMenu(bool isPaused)
    {
        // No pause menu in multiplayer
    }

    private void OnGiveUpClicked()
    {
        if (activeController != null)
        {
            activeController.Surrender();
        }
    }

    public void OnBackToMainMenuClicked()
    {
        Time.timeScale = 1f;
        if (FMODAudioManager.Instance != null)
        {
            FMODAudioManager.Instance.PlayMenuTheme();
        }

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        
        Debug.Log("[MultiplayerUI] Back to Main Menu, disconnecting from Photon...");
        SceneManager.LoadScene("MainMenu");
    }
}
