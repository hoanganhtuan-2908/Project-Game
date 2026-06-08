using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private const string LEVEL = "level";
    private const string TEAM = "team";
    private const byte MAX_PLAYERS = 2;

    [SerializeField] private ChessUIManager uiManager;
    [SerializeField] private GameInitializer gameInitializer;

    private MultiplayerChessGameController chessGameController;
    private ChessLevel playerLevel;

    private bool isTryingToConnect;

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void SetDependencies(MultiplayerChessGameController chessGameController)
    {
        this.chessGameController = chessGameController;
    }

    public void Connect()
    {
        if (isTryingToConnect)
        {
            Debug.LogWarning("Already trying to connect. Please wait...");
            return;
        }

        isTryingToConnect = true;

        // Nếu đang ở trong room rồi thì không được JoinRandomRoom nữa
        if (PhotonNetwork.InRoom)
        {
            Debug.LogWarning("Already in a room.");
            isTryingToConnect = false;
            return;
        }

        // Nếu chưa connect Photon thì connect trước
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Connecting to Photon...");
            PhotonNetwork.ConnectUsingSettings();
            return;
        }

        // Nếu đã connect và đang sẵn sàng ở Master Server
        if (PhotonNetwork.IsConnectedAndReady)
        {
            TryJoinRandomRoom();
        }
        else
        {
            Debug.LogWarning($"Photon is connected but not ready yet. Current state: {PhotonNetwork.NetworkClientState}");
            isTryingToConnect = false;
        }
    }

    private void TryJoinRandomRoom()
    {
        Debug.Log($"Looking for random room with level {playerLevel}");

        PhotonNetwork.JoinRandomRoom(
            new Hashtable()
            {
                { LEVEL, (int)playerLevel }
            },
            MAX_PLAYERS
        );
    }

    private void Update()
    {
        if (uiManager != null)
        {
            uiManager.SetConnectionStatusText(PhotonNetwork.NetworkClientState.ToString());
        }
    }

    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log($"Connected to Master Server. Looking for room with level {playerLevel}");
        TryJoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"Join random room failed: {message}. Creating new room...");

        PhotonNetwork.CreateRoom(
            null,
            new RoomOptions
            {
                MaxPlayers = MAX_PLAYERS,
                CustomRoomPropertiesForLobby = new string[] { LEVEL },
                CustomRoomProperties = new Hashtable()
                {
                    { LEVEL, (int)playerLevel }
                }
            }
        );
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room created successfully.");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Create room failed: {message}");
        isTryingToConnect = false;
    }

    public override void OnJoinedRoom()
    {
        isTryingToConnect = false;

        Debug.Log($"Player {PhotonNetwork.LocalPlayer.ActorNumber} joined room.");

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(LEVEL))
        {
            int roomLevel = (int)PhotonNetwork.CurrentRoom.CustomProperties[LEVEL];
            Debug.Log($"Room level: {(ChessLevel)roomLevel}");
        }

        gameInitializer.CreateMultiplayerBoard();
        PrepareTeamSelectionOptions();
        uiManager.ShowTeamSelectionScreen();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.ActorNumber} entered the room");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Disconnected from Photon: {cause}");
        isTryingToConnect = false;
    }

    #endregion

    private void PrepareTeamSelectionOptions()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            Player player = PhotonNetwork.CurrentRoom.GetPlayer(1);

            if (player != null && player.CustomProperties.ContainsKey(TEAM))
            {
                int occupiedTeam = (int)player.CustomProperties[TEAM];
                uiManager.RestrictTeamChoice((TeamColor)occupiedTeam);
            }
        }
    }

    public void SetPlayerLevel(ChessLevel level)
    {
        playerLevel = level;

        if (PhotonNetwork.LocalPlayer != null)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(
                new Hashtable()
                {
                    { LEVEL, (int)level }
                }
            );
        }
    }

    public void SetPlayerTeam(int teamInt)
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("Cannot set team because player is not in room.");
            return;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            Player player = PhotonNetwork.CurrentRoom.GetPlayer(1);

            if (player != null && player.CustomProperties.ContainsKey(TEAM))
            {
                int occupiedTeam = (int)player.CustomProperties[TEAM];
                teamInt = occupiedTeam == 0 ? 1 : 0;
            }
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(
            new Hashtable()
            {
            { TEAM, teamInt }
            }
        );

        MultiplayerChessGameController controller = gameInitializer.InitializeMultiplayerController();

        if (controller == null)
        {
            Debug.LogError("Cannot start game because MultiplayerChessGameController was not initialized.");
            return;
        }

        chessGameController = controller;

        TeamColor selectedTeam = (TeamColor)teamInt;

        chessGameController.SetupCamera(selectedTeam);
        chessGameController.SetLocalPlayer(selectedTeam);
        chessGameController.StartNewGame();
    }

    internal bool IsRoomFull()
    {
        return PhotonNetwork.InRoom &&
               PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers;
    }
}