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
    [SerializeField] private MultiplayerUIManager uiManager;
    [SerializeField] private MultiplayerChessGameController multiplayerControllerPrefab;

    private const string TEAM_PROP_KEY = "Team";
    private const string SKIN_PROP_KEY = "Skin";
    private const string READY_PROP_KEY = "Ready";
    private const string GAME_STARTED_ROOM_KEY = "GameStarted";

    private bool gameHasStarted = false;

    private IEnumerator Start()
    {
        if (setupUI == null)
        {
            Debug.LogError("[MultiplayerSceneInitializer] setupUI is not assigned!");
            yield break;
        }

        setupUI.gameObject.SetActive(true);

        // Setup Skin Dropdown options from GameSetupUI
        PopulateSkinDropdown();

        // Setup Dropdown option text for color choice (0 = White, 1 = Black)
        InitializeDropdownOptions();

        if (setupUI.BackToLobbyButton != null)
        {
            setupUI.BackToLobbyButton.onClick.AddListener(LeaveRoomAndGoToLobby);
        }

        // Wait until we are InRoom if we are connected to Photon to avoid timing issues on Guest client
        if (PhotonNetwork.IsConnected && !PhotonNetwork.InRoom)
        {
            float timeout = 3f;
            while (!PhotonNetwork.InRoom && timeout > 0)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
        }

        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[MultiplayerSceneInitializer] Not in a Photon Room! Are you running the scene directly from the Editor?");
            setupUI.RoomName.text = "OFFLINE TEST";
            setupUI.RoomID.text = "OFFLINE TEST";
            setupUI.PlayerName1.text = "Local Player";
            setupUI.PlayerName2.text = "No opponent";
            if (setupUI.StartGameButton != null)
            {
                setupUI.StartGameButton.gameObject.SetActive(true);
                setupUI.StartGameButton.interactable = false;
            }
            if (setupUI.ReadyButton != null)
            {
                setupUI.ReadyButton.gameObject.SetActive(false);
            }
            yield break;
        }

        // Initialize Room and Player Info
        setupUI.RoomName.text = PhotonNetwork.CurrentRoom.Name;
        setupUI.RoomID.text = "ID: " + PhotonNetwork.CurrentRoom.Name;

        // Set default player name if not set
        if (string.IsNullOrEmpty(PhotonNetwork.LocalPlayer.NickName))
        {
            PhotonNetwork.LocalPlayer.NickName = "Player_" + PhotonNetwork.LocalPlayer.ActorNumber;
        }

        // Set initial custom properties
        if (PhotonNetwork.IsMasterClient)
        {
            // Host defaults to White (0)
            SetLocalPlayerProperty(TEAM_PROP_KEY, 0);

            // Initialize Room skin property to 0
            ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable();
            roomProps[SKIN_PROP_KEY] = 0;
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
        }
        else
        {
            // Guest defaults to not ready (false)
            SetLocalPlayerProperty(READY_PROP_KEY, false);
        }

        // Setup Button listeners based on role
        if (setupUI.StartGameButton != null)
        {
            setupUI.StartGameButton.onClick.RemoveAllListeners();
            setupUI.StartGameButton.onClick.AddListener(RequestStartGame);
        }

        if (setupUI.ReadyButton != null)
        {
            setupUI.ReadyButton.onClick.RemoveAllListeners();
            setupUI.ReadyButton.onClick.AddListener(ToggleReadyState);
        }

        // Setup dropdown interactability based on host/guest role
        if (setupUI.ChooseBlackOrWhiteDropdown1 != null)
        {
            setupUI.ChooseBlackOrWhiteDropdown1.interactable = PhotonNetwork.IsMasterClient;
        }

        UpdateLobbyUI();
    }

    private void InitializeDropdownOptions()
    {
        if (setupUI.ChooseBlackOrWhiteDropdown1 != null)
        {
            setupUI.ChooseBlackOrWhiteDropdown1.ClearOptions();
            setupUI.ChooseBlackOrWhiteDropdown1.AddOptions(new List<string> { "White (Trắng)", "Black (Đen)" });
            setupUI.ChooseBlackOrWhiteDropdown1.onValueChanged.AddListener(OnHostColorDropdownChanged);
        }

        if (setupUI.ChooseBlackOrWhiteDropdown2 != null)
        {
            setupUI.ChooseBlackOrWhiteDropdown2.ClearOptions();
            setupUI.ChooseBlackOrWhiteDropdown2.AddOptions(new List<string> { "White (Trắng)", "Black (Đen)" });
            setupUI.ChooseBlackOrWhiteDropdown2.interactable = false;
        }

        if (setupUI.ChooseSkinDropdown != null)
        {
            setupUI.ChooseSkinDropdown.onValueChanged.AddListener(OnSkinDropdownChanged);
        }
    }

    private void PopulateSkinDropdown()
    {
        if (setupUI.ChooseSkinDropdown == null) return;

        setupUI.ChooseSkinDropdown.ClearOptions();

        List<string> options = new List<string>();

        if (setupUI.AvailableSkins != null && setupUI.AvailableSkins.Count > 0)
        {
            foreach (var skin in setupUI.AvailableSkins)
            {
                options.Add(string.IsNullOrEmpty(skin.skinName) ? skin.name : skin.skinName);
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
        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable();
            roomProps[SKIN_PROP_KEY] = index;
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
        }
    }

    private void ToggleReadyState()
    {
        bool currentReady = IsLocalPlayerReady();
        SetLocalPlayerProperty(READY_PROP_KEY, !currentReady);
    }

    private bool IsGuestReady()
    {
        Player guest = GetGuestPlayer();
        if (guest == null) return false;

        if (guest.CustomProperties.TryGetValue(READY_PROP_KEY, out object readyObj))
        {
            return (bool)readyObj;
        }
        return false;
    }

    private bool IsLocalPlayerReady()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(READY_PROP_KEY, out object readyObj))
        {
            return (bool)readyObj;
        }
        return false;
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

        // Sync Skin Dropdown from Room Custom Properties
        int selectedSkinIndex = 0;
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(SKIN_PROP_KEY, out object skinObj))
        {
            selectedSkinIndex = (int)skinObj;
        }
        if (setupUI.ChooseSkinDropdown != null)
        {
            setupUI.ChooseSkinDropdown.onValueChanged.RemoveListener(OnSkinDropdownChanged);
            setupUI.ChooseSkinDropdown.value = selectedSkinIndex;
            setupUI.ChooseSkinDropdown.RefreshShownValue();
            setupUI.ChooseSkinDropdown.onValueChanged.AddListener(OnSkinDropdownChanged);
            setupUI.ChooseSkinDropdown.interactable = PhotonNetwork.IsMasterClient;
        }

        // 3. Start Button / Ready Button Text & Interactability
        bool isHost = PhotonNetwork.IsMasterClient;

        if (setupUI.StartGameButton != null)
        {
            setupUI.StartGameButton.gameObject.SetActive(isHost);
            if (isHost)
            {
                // Host can only start if 2 players are present AND the guest is ready
                setupUI.StartGameButton.interactable = (PhotonNetwork.CurrentRoom.PlayerCount == 2) && IsGuestReady();
            }
        }

        if (setupUI.ReadyButton != null)
        {
            setupUI.ReadyButton.gameObject.SetActive(!isHost);
            if (!isHost)
            {
                bool isReady = IsLocalPlayerReady();
                TMP_Text readyText = setupUI.ReadyButton.GetComponentInChildren<TMP_Text>();
                if (readyText != null)
                {
                    readyText.text = isReady ? "Unready" : "Ready";
                }
                setupUI.ReadyButton.interactable = true;
            }
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

        // Verify again that the room is full and guest is ready
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && IsGuestReady())
        {
            Debug.Log("[Lobby] Host requested game start.");
            ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable();
            roomProps[GAME_STARTED_ROOM_KEY] = true;
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
        }
        else
        {
            Debug.LogWarning("[Lobby] Cannot start game: Opponent is not ready or has left.");
        }
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
        if (changedProps.ContainsKey(TEAM_PROP_KEY) || changedProps.ContainsKey(SKIN_PROP_KEY) || changedProps.ContainsKey(READY_PROP_KEY))
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
        if (propertiesThatChanged.ContainsKey(SKIN_PROP_KEY))
        {
            UpdateLobbyUI();
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

        // Apply skin choice decided by the Host (stored in Room properties)
        int localSkinIndex = 0;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(SKIN_PROP_KEY, out object skinObj))
        {
            localSkinIndex = (int)skinObj;
        }

        // Apply skin from dropdown choice to PiecesCreator on controller
        PiecesCreator piecesCreator = controller.GetComponent<PiecesCreator>();
        if (piecesCreator != null && setupUI != null && setupUI.AvailableSkins != null && localSkinIndex < setupUI.AvailableSkins.Count)
        {
            piecesCreator.SetActiveSkin(setupUI.AvailableSkins[localSkinIndex]);
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

        uiManager.OnGameStarted();

        controller.StartNewGame();
        Debug.Log($"[Lobby] Game started! Local Team: {localTeam}");
    }

    #endregion
}
