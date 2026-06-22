using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private const string LEVEL = "level";
    private const string TEAM = "team";
    private const byte MAX_PLAYERS = 2;

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

        if (PhotonNetwork.InRoom)
        {
            Debug.LogWarning("Already in a room.");
            isTryingToConnect = false;
            return;
        }

        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Connecting to Photon...");
            PhotonNetwork.ConnectUsingSettings();
            return;
        }

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

    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log($"Connected to Master Server.");
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

    public bool IsRoomFull()
    {
        return PhotonNetwork.InRoom &&
               PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers;
    }
}