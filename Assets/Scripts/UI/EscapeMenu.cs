using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EscapeMenu : MonoBehaviour
{
    public static EscapeMenu Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject menuPanel; // The panel that opens when Esc is pressed

    [Header("Buttons (Optional)")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("Audio Settings (Optional)")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle muteToggle;

    private void Awake()
    {
        Instance = this;

        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
    }

    private void Start()
    {
        // Wire up buttons if assigned
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitToMainMenu);
        }

        // Initialize audio settings if assigned
        InitializeAudioSettings();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        if (menuPanel == null) return;

        bool nextState = !menuPanel.activeSelf;
        menuPanel.SetActive(nextState);

        // Pause/Resume game time in Singleplayer scene when menu is toggled
        if (SceneManager.GetActiveScene().name == "SinglePlayer")
        {
            Time.timeScale = nextState ? 0f : 1f;
        }
    }

    public void ResumeGame()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
        if (SceneManager.GetActiveScene().name == "SinglePlayer")
        {
            Time.timeScale = 1f;
        }
    }

    public void RestartGame()
    {
        ResumeGame();
        var controller = FindAnyObjectByType<ChessGameController>();
        if (controller != null)
        {
            controller.RestartGame();
        }
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        
        // Play main menu BGM
        if (FMODAudioManager.Instance != null)
        {
            FMODAudioManager.Instance.PlayMenuTheme();
        }

        // Leave room if in multiplayer
        if (Photon.Pun.PhotonNetwork.InRoom)
        {
            Photon.Pun.PhotonNetwork.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    private void InitializeAudioSettings()
    {
        if (volumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (muteToggle != null)
        {
            bool savedMute = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
            muteToggle.isOn = savedMute;
            muteToggle.onValueChanged.AddListener(OnMuteToggled);
        }
    }

    private void OnVolumeChanged(float value)
    {
        if (FMODAudioManager.Instance != null)
        {
            FMODAudioManager.Instance.SetVolume(value);
        }
        else
        {
            PlayerPrefs.SetFloat("MusicVolume", value);
            PlayerPrefs.Save();
        }
    }

    private void OnMuteToggled(bool isMuted)
    {
        if (FMODAudioManager.Instance != null)
        {
            FMODAudioManager.Instance.SetMuted(isMuted);
        }
        else
        {
            PlayerPrefs.SetInt("MusicMuted", isMuted ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
