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
    [SerializeField] private TMP_Dropdown teamSelectionDropdown;

    private void Start()
    {
        if (teamSelectionDropdown != null)
        {
            teamSelectionDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<string> { "White (Trắng)", "Black (Đen)" };
            teamSelectionDropdown.AddOptions(options);
            teamSelectionDropdown.value = 0;
            teamSelectionDropdown.RefreshShownValue();
        }

        if (setupPanel != null)
        {
            setupPanel.SetActive(true);
        }
        else
        {
            StartGame();
        }
    }

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
            board = FindAnyObjectByType<SinglePlayerBoard>();
        }

        if (board == null)
        {
            Debug.LogError("[SingleplayerSceneInitializer] SinglePlayerBoard not found in the scene! Please make sure it is placed in the scene Hierarchy.");
            return;
        }

        if (cameraSetup == null)
        {
            cameraSetup = FindAnyObjectByType<CameraSetup>();
        }

        if (uiManager == null)
        {
            uiManager = FindAnyObjectByType<SingleplayerUIManager>();
        }

        IChessUIManager resolvedUIManager = ResolveUIManager();

        if (resolvedUIManager == null)
        {
            Debug.LogError("[SingleplayerSceneInitializer] No UI manager implementing IChessUIManager was found in the scene!");
            return;
        }

        if (singleplayerControllerPrefab == null)
        {
            Debug.LogError("[SingleplayerSceneInitializer] SingleplayerChessGameController prefab is not assigned in the inspector!");
            return;
        }

        TeamColor selectedTeam = TeamColor.White;
        if (teamSelectionDropdown != null)
        {
            selectedTeam = teamSelectionDropdown.value == 0 ? TeamColor.White : TeamColor.Black;
        }
        Debug.Log($"[SingleplayerSceneInitializer] Starting game as: {selectedTeam}");

        SingleplayerChessGameController controller = Instantiate(singleplayerControllerPrefab);

        controller.SetLocalPlayerTeam(selectedTeam);

        ChessSkin selectedSkin = GetSelectedSkin(resolvedUIManager);
        if (selectedSkin != null)
        {
            PiecesCreator creator = controller.GetComponent<PiecesCreator>();
            if (creator != null)
            {
                creator.SetActiveSkin(selectedSkin);
            }
        }

        controller.SetDependencies(cameraSetup, resolvedUIManager, board);
        controller.InitializeGame();

        board.SetDependencies(controller);
        SetActiveController(resolvedUIManager, controller);

        controller.StartNewGame();
        controller.SetupCamera(selectedTeam);
    }

    private IChessUIManager ResolveUIManager()
    {
        if (uiManager != null)
            return uiManager;

        ChessUIManager sharedUIManager = FindAnyObjectByType<ChessUIManager>();
        if (sharedUIManager != null)
            return sharedUIManager;

        return FindAnyObjectByType<SingleplayerUIManager>();
    }

    private ChessSkin GetSelectedSkin(IChessUIManager resolvedUIManager)
    {
        if (resolvedUIManager is SingleplayerUIManager singleplayerUIManager)
            return singleplayerUIManager.SelectedSkin;

        if (resolvedUIManager is ChessUIManager sharedUIManager)
            return sharedUIManager.SelectedSkin;

        return null;
    }

    private void SetActiveController(IChessUIManager resolvedUIManager, ChessGameController controller)
    {
        if (resolvedUIManager is SingleplayerUIManager singleplayerUIManager)
        {
            singleplayerUIManager.SetActiveController(controller);
            return;
        }

        if (resolvedUIManager is ChessUIManager sharedUIManager)
        {
            sharedUIManager.SetActiveController(controller);
        }
    }
}
