using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MultiplayerSceneInitializer : MonoBehaviourPunCallbacks
{
    [Header("UI Panels")]
    [SerializeField] private GameSetupUI setupUI;

    [Header("Dependencies")]
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private Transform multiplayerBoardAnchor;
    [SerializeField] private CameraSetup cameraSetup;
    [SerializeField] private ChessUIManager uiManager;
    [SerializeField] private MultiplayerChessGameController multiplayerControllerPrefab;

    private const string TEAM_PROP_KEY = "Team";
    private const string SKIN_PROP_KEY = "Skin";
    private const string GAME_STARTED_ROOM_KEY = "GameStarted";

    private bool gameHasStarted = false;

    private void Start()
    {
        if (setupUI == null)
        {
            Debug.LogError("[MultiplayerSceneInitializer] setupUI is not assigned!");
            return;
        }

        setupUI.gameObject.SetActive(true);

        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[MultiplayerSceneInitializer] Not in a Photon Room! Are you running the scene directly from the Editor?");
            setupUI.RoomName.text = "OFFLINE TEST";
            setupUI.RoomID.text = "OFFLINE TEST";
            setupUI.PlayerName1.text = "Local Player";
            setupUI.PlayerName2.text = "No opponent";
            setupUI.StartGameButton.interactable = false;
            return;
        }

        // Initialize Room and Player Info
        setupUI.RoomName.text = PhotonNetwork.CurrentRoom.Name;
        setupUI.RoomID.text = "ID: " + PhotonNetwork.CurrentRoom.Name;

        // Set default player name if not set
        if (string.IsNullOrEmpty(PhotonNetwork.LocalPlayer.NickName))
        {
            PhotonNetwork.LocalPlayer.NickName = "Player_" + PhotonNetwork.LocalPlayer.ActorNumber;
        }

        // Setup Skin Dropdown options from ChessUIManager
        PopulateSkinDropdown();

        // Setup Dropdown listener for Master Client (Host) to choose color
        if (setupUI.ChooseBlackOrWhiteDropdown1 != null)
        {
            setupUI.ChooseBlackOrWhiteDropdown1.ClearOptions();
            setupUI.ChooseBlackOrWhiteDropdown1.AddOptions(new List<string> { "White (Trắng)", "Black (Đen)" });
            setupUI.ChooseBlackOrWhiteDropdown1.onValueChanged.AddListener(OnHostColorDropdownChanged);
            setupUI.ChooseBlackOrWhiteDropdown1.interactable = PhotonNetwork.IsMasterClient;
        }

        if (setupUI.ChooseBlackOrWhiteDropdown2 != null)
        {
            setupUI.ChooseBlackOrWhiteDropdown2.ClearOptions();
            setupUI.ChooseBlackOrWhiteDropdown2.AddOptions(new List<string> { "White (Trắng)", "Black (Đen)" });
            // Guest's dropdown is strictly read-only and automatically updated based on Host's choice
            setupUI.ChooseBlackOrWhiteDropdown2.interactable = false;
        }

        if (setupUI.ChooseSkinDropdown != null)
        {
            setupUI.ChooseSkinDropdown.onValueChanged.AddListener(OnSkinDropdownChanged);
        }

        // Setup Button listeners
        if (setupUI.StartGameButton != null)
        {
            setupUI.StartGameButton.onClick.AddListener(RequestStartGame);
        }

        if (setupUI.BackToLobbyButton != null)
        {
            setupUI.BackToLobbyButton.onClick.AddListener(LeaveRoomAndGoToLobby);
        }

        // Set initial properties
        if (PhotonNetwork.IsMasterClient)
        {
            // Host defaults to White (0)
            SetLocalPlayerProperty(TEAM_PROP_KEY, 0);
        }
        // Set default skin index (0)
        SetLocalPlayerProperty(SKIN_PROP_KEY, 0);

        UpdateLobbyUI();
    }

    private void PopulateSkinDropdown()
    {
        if (setupUI.ChooseSkinDropdown == null || uiManager == null) return;

        setupUI.ChooseSkinDropdown.ClearOptions();
        
        // Find available skins (we can inspect uiManager field)
        // Let's check ChessUIManager dropdown or list
        List<string> options = new List<string>();
        // Reflection-safe or fallback
        // The ChessUIManager has public List<ChessSkin> availableSkins or setup through dropdown
        // Let's get the list if possible, or use standard options
        // We know ChessUIManager.cs line 34: List<ChessSkin> availableSkins;
        // Let's populate from there if uiManager has them
        // If not, we fall back to generic
        var skinDropdownInUIManager = uiManager.GetComponentInChildren<TMP_Dropdown>();
        if (skinDropdownInUIManager != null)
        {
            foreach (var opt in skinDropdownInUIManager.options)
            {
                options.Add(opt.text);
            }
        }
        else
        {
            options.Add("Default Skin");
        }

        setupUI.ChooseSkinDropdown.AddOptions(options);
    }

    private void SetLocalPlayerProperty(string key, object value)
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props[key] = value;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    private void OnHostColorDropdownChanged(int index)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SetLocalPlayerProperty(TEAM_PROP_KEY, index);
        }
    }

    private void OnSkinDropdownChanged(int index)
    {
        SetLocalPlayerProperty(SKIN_PROP_KEY, index);
    }

    private void UpdateLobbyUI()
    {
        if (setupUI == null || !PhotonNetwork.InRoom) return;

        Player host = PhotonNetwork.MasterClient;
        Player guest = GetGuestPlayer();

        // 1. Display Names
        setupUI.PlayerName1.text = host != null ? host.NickName : "Waiting for Host...";
        setupUI.PlayerName2.text = guest != null ? guest.NickName : "Waiting for Opponent...";

        // 2. Synchronize Color Dropdowns
        int hostColorIndex = 0; // 0 = White, 1 = Black
        if (host != null && host.CustomProperties.TryGetValue(TEAM_PROP_KEY, out object teamObj))
        {
            hostColorIndex = (int)teamObj;
        }

        if (setupUI.ChooseBlackOrWhiteDropdown1 != null)
        {
            setupUI.ChooseBlackOrWhiteDropdown1.value = hostColorIndex;
            setupUI.ChooseBlackOrWhiteDropdown1.RefreshShownValue();
        }

        if (setupUI.ChooseBlackOrWhiteDropdown2 != null)
        {
            setupUI.ChooseBlackOrWhiteDropdown2.value = 1 - hostColorIndex; // Guest gets opposite
            setupUI.ChooseBlackOrWhiteDropdown2.RefreshShownValue();
        }

        // 3. Start Button Interactability
        if (setupUI.StartGameButton != null)
        {
            // Only host can start, and only when 2 players are present
            setupUI.StartGameButton.interactable = PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2;
        }
    }

    private Player GetGuestPlayer()
    {
        foreach (var playerEntry in PhotonNetwork.CurrentRoom.Players)
        {
            if (playerEntry.Value != PhotonNetwork.MasterClient)
            {
                return playerEntry.Value;
            }
        }
        return null;
    }

    private void RequestStartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log("[Lobby] Host requested game start.");
        ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable();
        roomProps[GAME_STARTED_ROOM_KEY] = true;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
    }

    private void LeaveRoomAndGoToLobby()
    {
        Debug.Log("[Lobby] Leaving room...");
        PhotonNetwork.LeaveRoom();
    }

    #region Photon Callbacks

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[Lobby] Player entered: {newPlayer.NickName}");
        UpdateLobbyUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"[Lobby] Player left: {otherPlayer.NickName}");
        if (gameHasStarted)
        {
            Debug.Log("[Lobby] Game in progress was interrupted because a player left.");
            // Handle player disconnect during game (e.g. notify game controller or end game)
            if (uiManager != null)
            {
                uiManager.OnGameFinished(PhotonNetwork.LocalPlayer.NickName);
            }
        }
        else
        {
            UpdateLobbyUI();
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey(TEAM_PROP_KEY) || changedProps.ContainsKey(SKIN_PROP_KEY))
        {
            UpdateLobbyUI();
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(GAME_STARTED_ROOM_KEY) && (bool)propertiesThatChanged[GAME_STARTED_ROOM_KEY])
        {
            if (!gameHasStarted)
            {
                StartCoroutine(StartChessGameSequence());
            }
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("[Lobby] Left room. Loading Lobby Scene.");
        SceneManager.LoadScene("MultiplayerPlayerLobbySence");
    }

    #endregion

    #region Game Start Sequence

    private IEnumerator StartChessGameSequence()
    {
        gameHasStarted = true;
        setupUI.gameObject.SetActive(false);

        Debug.Log("[Lobby] Initializing board and controller...");

        // 1. Host creates the board
        if (PhotonNetwork.IsMasterClient)
        {
            if (FindAnyObjectByType<MultiplayerBoard>() == null)
            {
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
        }

        // 2. Wait until MultiplayerBoard is found across network
        MultiplayerBoard board = null;
        while (board == null)
        {
            board = FindAnyObjectByType<MultiplayerBoard>();
            yield return null;
        }

        // Ensure board is parented and positioned correctly on both clients
        if (board.transform.parent != multiplayerBoardAnchor)
        {
            board.transform.SetParent(multiplayerBoardAnchor);
            board.transform.localPosition = Vector3.zero;
            board.transform.localRotation = Quaternion.identity;
            board.transform.localScale = Vector3.one;
        }

        // 3. Initialize Controller
        MultiplayerChessGameController controller = Instantiate(multiplayerControllerPrefab);
        if (controller == null)
        {
            Debug.LogError("[MultiplayerSceneInitializer] Failed to instantiate controller!");
            yield break;
        }

        // 4. Assign Team Color
        Player host = PhotonNetwork.MasterClient;
        int hostColorIndex = 0;
        if (host != null && host.CustomProperties.TryGetValue(TEAM_PROP_KEY, out object teamObj))
        {
            hostColorIndex = (int)teamObj;
        }

        TeamColor localTeam = TeamColor.White;
        if (PhotonNetwork.IsMasterClient)
        {
            localTeam = hostColorIndex == 0 ? TeamColor.White : TeamColor.Black;
        }
        else
        {
            localTeam = hostColorIndex == 0 ? TeamColor.Black : TeamColor.White;
        }

        // Apply skin choice
        int localSkinIndex = 0;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(SKIN_PROP_KEY, out object skinObj))
        {
            localSkinIndex = (int)skinObj;
        }

        // Apply skin from dropdown choice to PiecesCreator on controller
        PiecesCreator piecesCreator = controller.GetComponent<PiecesCreator>();
        if (piecesCreator != null && uiManager != null && uiManager.AvailableSkins != null && localSkinIndex < uiManager.AvailableSkins.Count)
        {
            piecesCreator.SetActiveSkin(uiManager.AvailableSkins[localSkinIndex]);
        }

        // Bind dependencies directly
        controller.SetDependencies(cameraSetup, uiManager, board);
        controller.InitializeGame();
        controller.SetNetworkManager(networkManager);

        networkManager.SetDependencies(controller);
        board.SetDependencies(controller);
        uiManager.SetActiveController(controller);

        // Setup game start on controller
        controller.SetupCamera(localTeam);
        controller.SetLocalPlayer(localTeam);

        if (uiManager != null)
        {
            uiManager.OnGameStarted();
        }

        controller.StartNewGame();
        Debug.Log($"[Lobby] Game started! Local Team: {localTeam}");
    }

    #endregion
}
