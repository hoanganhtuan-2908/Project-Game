using UnityEngine;
using TMPro;

public class SingleplayerSceneInitializer : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private SingleplayerChessGameController singleplayerControllerPrefab;

    [Header("Scene References")]
    [SerializeField] private CameraSetup cameraSetup;
    [SerializeField] private SingleplayerUIManager uiManager;
    [SerializeField] private SinglePlayerBoard board;

    [Header("Singleplayer Setup UI")]
    [SerializeField] private GameObject setupPanel;
    [SerializeField] private TMP_Dropdown teamSelectionDropdown; // Dropdown để chọn bên (0 = White, 1 = Black)

    private void Start()
    {
        // Tự động điền các tùy chọn "White" và "Black" cho Dropdown chọn bên
        if (teamSelectionDropdown != null)
        {
            teamSelectionDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<string> { "White (Trắng)", "Black (Đen)" };
            teamSelectionDropdown.AddOptions(options);
            teamSelectionDropdown.value = 0; // Mặc định chọn quân Trắng
            teamSelectionDropdown.RefreshShownValue();
        }

        // 1. Hiển thị bảng cài đặt và đợi người chơi chọn các thông số
        if (setupPanel != null)
        {
            setupPanel.SetActive(true);
        }
        else
        {
            // Nếu không gán panel, tự động bắt đầu game với quân Trắng
            StartGame();
        }
    }

    /// <summary>
    /// Được gọi bởi nút "Start Game" trên bảng cài đặt
    /// </summary>
    public void StartGame()
    {
        if (setupPanel != null)
        {
            setupPanel.SetActive(false);
        }

        InitializeGame();
    }

    private void InitializeGame()
    {
        if (board == null)
        {
            board = FindObjectOfType<SinglePlayerBoard>();
        }

        if (board == null)
        {
            Debug.LogError("[SingleplayerSceneInitializer] SinglePlayerBoard not found in the scene! Please make sure it is placed in the scene Hierarchy.");
            return;
        }

        if (cameraSetup == null)
        {
            cameraSetup = FindObjectOfType<CameraSetup>();
        }

        if (uiManager == null)
        {
            uiManager = FindObjectOfType<SingleplayerUIManager>();
        }

        if (singleplayerControllerPrefab == null)
        {
            Debug.LogError("[SingleplayerSceneInitializer] SingleplayerChessGameController prefab is not assigned in the inspector!");
            return;
        }

        // 1. Xác định phe người chơi từ Dropdown (0 = White, 1 = Black)
        TeamColor selectedTeam = TeamColor.White;
        if (teamSelectionDropdown != null)
        {
            selectedTeam = (teamSelectionDropdown.value == 0) ? TeamColor.White : TeamColor.Black;
        }
        Debug.Log($"[SingleplayerSceneInitializer] Bắt đầu chơi với phe: {selectedTeam}");

        // 2. Tạo Controller cho Singleplayer
        SingleplayerChessGameController controller = Instantiate(singleplayerControllerPrefab);

        // 3. Thiết lập phe của người chơi cho Controller
        controller.SetLocalPlayerTeam(selectedTeam);

        // 4. Áp dụng skin được chọn từ UI Manager
        if (uiManager != null && uiManager.SelectedSkin != null)
        {
            PiecesCreator creator = controller.GetComponent<PiecesCreator>();
            if (creator != null)
            {
                creator.SetActiveSkin(uiManager.SelectedSkin);
            }
        }

        // 5. Thiết lập các liên kết dependency và khởi tạo game
        controller.SetDependencies(cameraSetup, uiManager, board);
        controller.InitializeGame();

        board.SetDependencies(controller);

        if (uiManager != null)
        {
            uiManager.SetActiveController(controller);
            uiManager.OnGameStarted(); // Chuyển giao diện sang trạng thái đang chơi
        }

        // 6. Bắt đầu trận đấu
        controller.StartNewGame();

        // 7. Thiết lập Camera xoay về hướng của phe người chơi chọn
        controller.SetupCamera(selectedTeam);
    }
}
