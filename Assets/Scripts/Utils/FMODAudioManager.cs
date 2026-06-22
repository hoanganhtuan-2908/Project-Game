using UnityEngine;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;

public class FMODAudioManager : MonoBehaviour
{
    public static FMODAudioManager Instance { get; private set; }

    [Header("FMOD Events")]
    [SerializeField] private EventReference menuThemeEvent;
    [SerializeField] private List<EventReference> gameplayBGMEvents; // List of gameplay BGM events
    [SerializeField] private EventReference winThemeEvent;
    [SerializeField] private EventReference loseThemeEvent;

    [Header("SFX Events")]
    [SerializeField] private EventReference selfMoveEvent;
    [SerializeField] private EventReference oppMoveEvent;
    [SerializeField] private EventReference captureEvent;
    [SerializeField] private EventReference castleEvent;
    [SerializeField] private EventReference checkedEvent;
    [SerializeField] private EventReference promoteEvent;
    [SerializeField] private EventReference illegalMoveEvent;
    [SerializeField] private EventReference buttonClickEvent;

    [Header("FMOD Mixer Routing")]
    [SerializeField] private string musicBusPath = "bus:/Music";


    private EventInstance currentMusicInstance;
    private Bus musicBus;

    private void Awake()
    {
        // Chỉ xử lý Singleton và DontDestroyOnLoad ở Awake() để đảm bảo an toàn
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        try
        {
            // Lấy Mixer Bus của FMOD ở Start (khi FMOD đã sẵn sàng)
            musicBus = RuntimeManager.GetBus(musicBusPath);
            // Áp dụng cấu hình âm lượng
            UpdateFMODAudioSettings();
            // Phát nhạc Menu
            PlayMenuTheme();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FMODAudioManager] Lỗi khởi tạo FMOD ở Start: {e.Message}");
        }
    }

    public void UpdateFMODAudioSettings()
    {
        bool isMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        float volume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);

        musicBus.setMute(isMuted);
        musicBus.setVolume(volume);
    }

    private void PlayEvent(EventReference fmodEvent)
    {
        Debug.Log($"[FMODAudioManager] PlayEvent called. IsNull: {fmodEvent.IsNull}, Path: {fmodEvent}");
        if (fmodEvent.IsNull) return;
        
        // Dừng bài hát hiện tại
        StopCurrentMusic();

        currentMusicInstance = RuntimeManager.CreateInstance(fmodEvent);
        Debug.Log($"[FMODAudioManager] Created instance for: {fmodEvent}");

        // Phát âm thanh tại vị trí của Camera chính để tránh bị nhỏ/tắt tiếng do 3D spatialization
        Transform targetTransform = Camera.main != null ? Camera.main.transform : transform;
        var rigidbody = targetTransform.GetComponent<Rigidbody>();
        RuntimeManager.AttachInstanceToGameObject(currentMusicInstance, targetTransform, rigidbody);

        FMOD.RESULT result = currentMusicInstance.start();
        Debug.Log($"[FMODAudioManager] Started instance. FMOD Result: {result}");
    }



    public void StopCurrentMusic()
    {
        if (currentMusicInstance.isValid())
        {
            currentMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            currentMusicInstance.release();
        }
    }


    public void PlayMenuTheme()
    {
        PlayEvent(menuThemeEvent);
    }


    public void PlayGameplayBGM()
    {
        if (gameplayBGMEvents != null && gameplayBGMEvents.Count > 0)
        {
            // Chọn ngẫu nhiên 1 trong các bài nhạc nền gameplay
            int randomIndex = Random.Range(0, gameplayBGMEvents.Count);
            EventReference selectedEvent = gameplayBGMEvents[randomIndex];
            PlayEvent(selectedEvent);
        }
        else
        {
            Debug.LogWarning("[FMODAudioManager] Danh sách Gameplay BGM trống!");
        }
    }


    public void PlayGameFinishedTheme(bool won)
    {
        Debug.Log($"[FMODAudioManager] PlayGameFinishedTheme called. won: {won}");
        if (won)
        {
            PlayEvent(winThemeEvent);
        }
        else
        {
            PlayEvent(loseThemeEvent);
        }
    }
    public void SetVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
        musicBus.setVolume(volume);
    }

    public void SetMuted(bool isMuted)
    {
        PlayerPrefs.SetInt("MusicMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();

        if (isMuted)
        {
            musicBus.setVolume(0f); // Tắt tiếng bằng cách đặt âm lượng về 0
        }
        else
        {
            // Khôi phục lại âm lượng đã lưu từ thanh cuộn (Slider)
            float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            musicBus.setVolume(savedVolume);
        }
    }
    private void OnDestroy()
    {
        StopCurrentMusic();
    }
    public void PlaySelfMove() => PlayOneShot(selfMoveEvent);
    public void PlayOppMove() => PlayOneShot(oppMoveEvent);
    public void PlayCapture() => PlayOneShot(captureEvent);
    public void PlayCastle() => PlayOneShot(castleEvent);
    public void PlayChecked() => PlayOneShot(checkedEvent);
    public void PlayPromote() => PlayOneShot(promoteEvent);
    public void PlayIllegalMove() => PlayOneShot(illegalMoveEvent);
    public void PlayButtonClick() => PlayOneShot(buttonClickEvent);
    private void PlayOneShot(EventReference fmodEvent)
    {
        if (!fmodEvent.IsNull)
        {
            RuntimeManager.PlayOneShot(fmodEvent);
        }
    }
}
