using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject mainmenuPanel;
    [SerializeField] private GameObject optionPanel;

    [Header("Music Settings UI")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle muteToggle;

    private void Start()
    {
        InitializeMusicSettings();
    }


    /// Chuyển sang scene chơi đơn (Singleplayer)
    public void PlaySinglePlayer()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SinglePlayer");
    }


    /// <summary>
    /// Kích hoạt giao diện kết nối mạng (Multiplayer)
    /// </summary>
    public void PlayMultiPlayer()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("AutoStartMultiplayer", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Multiplayer");
    }

    public void openOption()
    {
        if (mainmenuPanel != null) { mainmenuPanel.SetActive(false); }
        if (optionPanel != null) { optionPanel.SetActive(true); }
    }

    public void closeOption()
    {
        if (optionPanel != null) { optionPanel.SetActive(false); }
        if (mainmenuPanel != null) { mainmenuPanel.SetActive(true); }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void InitializeMusicSettings()
    {
        // Khởi tạo trạng thái Volume
        if (volumeSlider != null) 
        {
            float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        // Khởi tạo trạng thái Mute
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
