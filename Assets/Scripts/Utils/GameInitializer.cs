using UnityEngine;
using Photon.Pun;

public class GameInitializer : MonoBehaviour
{
    [Header("Game mode dependent objects")]
    [SerializeField] private SingleplayerChessGameController singleplayerControllerPrefab;
    [SerializeField] private MultiplayerChessGameController multiplayerControllerPrefab;
    [SerializeField] private MultiplayerBoard multiplayerBoardPrefab;
    [SerializeField] private SinglePlayerBoard singleplayerBoardPrefab;

    [Header("Scene references")]
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private CameraSetup cameraSetup;
    [SerializeField] private ChessUIManager uiManager;

    [Header("Board Anchors")]
    [SerializeField] private Transform singleplayerBoardAnchor;
    [SerializeField] private Transform multiplayerBoardAnchor;

    public void CreateMultiplayerBoard()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (FindObjectOfType<MultiplayerBoard>() != null)
            return;

        GameObject boardObject = PhotonNetwork.Instantiate(
            "MultiplayerBoard",
            multiplayerBoardAnchor.position,
            multiplayerBoardAnchor.rotation
        );

        boardObject.transform.SetParent(multiplayerBoardAnchor);
        boardObject.transform.localPosition = Vector3.zero;
        boardObject.transform.localRotation = Quaternion.identity;
        boardObject.transform.localScale = Vector3.one;
    }

    public void CreateSinglePlayerBoard()
    {
        SinglePlayerBoard board = Instantiate(
            singleplayerBoardPrefab,
            singleplayerBoardAnchor
        );

        board.transform.localPosition = Vector3.zero;
        board.transform.localRotation = Quaternion.identity;
        board.transform.localScale = Vector3.one;
    }

    public MultiplayerChessGameController InitializeMultiplayerController()
    {
        if (cameraSetup == null)
        {
            Debug.LogError("CameraSetup is missing in GameInitializer.");
            return null;
        }

        if (uiManager == null)
        {
            Debug.LogError("ChessUIManager is missing in GameInitializer.");
            return null;
        }

        MultiplayerBoard board = FindObjectOfType<MultiplayerBoard>();

        if (board == null)
        {
            Debug.LogError("MultiplayerBoard not found.");
            return null;
        }

        MultiplayerChessGameController controller = Instantiate(multiplayerControllerPrefab);

        // Apply selected skin from UI Manager
        if (uiManager != null && uiManager.SelectedSkin != null)
        {
            PiecesCreator creator = controller.GetComponent<PiecesCreator>();
            if (creator != null)
            {
                creator.SetActiveSkin(uiManager.SelectedSkin);
            }
        }

        controller.SetDependencies(cameraSetup, uiManager, board);
        controller.InitializeGame();
        controller.SetNetworkManager(networkManager);

        networkManager.SetDependencies(controller);
        board.SetDependencies(controller);
        uiManager.SetActiveController(controller);

        return controller;
    }

    public void InitializeSingleplayerController()
    {
        if (cameraSetup == null)
        {
            Debug.LogError("CameraSetup is missing in GameInitializer.");
            return;
        }

        if (uiManager == null)
        {
            Debug.LogError("ChessUIManager is missing in GameInitializer.");
            return;
        }

        SinglePlayerBoard board = FindObjectOfType<SinglePlayerBoard>();

        if (board == null)
        {
            Debug.LogError("SinglePlayerBoard not found.");
            return;
        }

        SingleplayerChessGameController controller = Instantiate(singleplayerControllerPrefab);

        // Apply selected skin from UI Manager
        if (uiManager != null && uiManager.SelectedSkin != null)
        {
            PiecesCreator creator = controller.GetComponent<PiecesCreator>();
            if (creator != null)
            {
                creator.SetActiveSkin(uiManager.SelectedSkin);
            }
        }

        controller.SetDependencies(cameraSetup, uiManager, board);
        controller.InitializeGame();

        board.SetDependencies(controller);
        uiManager.SetActiveController(controller);
        controller.StartNewGame();
    }
}