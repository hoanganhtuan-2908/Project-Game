using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class MultiplayerUIManager : MonoBehaviourPunCallbacks, IChessUIManager
{
    [Header("Screen GameObjects")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject victoryBackground;
    [SerializeField] private GameObject defeatBackground;

    [Header("Buttons")]
    [SerializeField] private Button giveUpButton;
    [SerializeField] private Button backToLobbyButton;

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
        if (backToLobbyButton != null)
        {
            backToLobbyButton.onClick.AddListener(OnBackToLobbyClicked);
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

    public void OnBackToLobbyClicked()
    {
        Time.timeScale = 1f;
        if (FMODAudioManager.Instance != null)
        {
            FMODAudioManager.Instance.PlayMenuTheme();
        }

        Debug.Log("[MultiplayerUI] Back to Lobby, leaving room...");
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene("MultiplayerPlayerLobbySence");
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("[MultiplayerUI] Left room. Loading Lobby Scene.");
        SceneManager.LoadScene("MultiplayerPlayerLobbySence");
    }
}
