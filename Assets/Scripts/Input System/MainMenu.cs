using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private ChessUIManager uiManager;

    /// <summary>
    /// Chuyển sang scene chơi đơn (Singleplayer)
    /// </summary>
    public void PlaySinglePlayer()
    {
        Time.timeScale = 1f;
        
        // Đảm bảo lưu thiết lập trước khi chuyển cảnh
        if (uiManager != null)
        {
            // ChessUIManager tự động lưu SelectedSkinIndex và SelectedLevel qua PlayerPrefs
            Debug.Log($"[MainMenu] Khởi động Singleplayer với Skin: {uiManager.SelectedSkin?.skinName}, Độ khó: {uiManager.SelectedLevel}");
        }

        SceneManager.LoadScene("SinglePlayer");
    }

    /// <summary>
    /// Kích hoạt giao diện kết nối mạng (Multiplayer)
    /// </summary>
    public void PlayMultiPlayer()
    {
        Time.timeScale = 1f;

        // Nếu bạn thiết kế sảnh chờ (Lobby) trực tiếp trên scene MainMenu hiện tại:
        if (uiManager != null)
        {
            uiManager.OnMultiPlayerModeSelected();
        }
        else
        {
            // Hoặc nếu sau này bạn muốn tách riêng 1 scene Lobby mạng:
            // SceneManager.LoadScene("MultiplayerLobby");
            Debug.LogWarning("[MainMenu] ChessUIManager is null. Không thể chuyển sang chế độ Multiplayer UI.");
        }
    }
}
