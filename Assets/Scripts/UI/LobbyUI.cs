using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviourPunCallbacks
{
    [Header("UI Buttons")]
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button joinWithCodeButton;
    [SerializeField] private Button joinLobbyButton; // Big Join button under the list

    [Header("UI Inputs")]
    [SerializeField] private TMP_InputField playerNameInput;

    [Header("Lobby Panels")]
    [SerializeField] private CreateLobbyUI createLobbyPanel;
    [SerializeField] private JoinRoomUI joinRoomPanel;

    [Header("Room List View")]
    [SerializeField] private Transform content;
    [SerializeField] private LobbyDetail lobbyItemPrefab; // Changed from LobbyListUI to LobbyDetail
    [SerializeField] private TMP_Text statusText;

    private Dictionary<string, RoomInfo> roomCache = new Dictionary<string, RoomInfo>();
    private List<GameObject> roomListGameObjects = new List<GameObject>();
    private LobbyDetail selectedLobbyDetail;

    private const string PLAYER_NAME_PREF_KEY = "LobbyPlayerName";

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        // Load player name preference
        if (playerNameInput != null)
        {
            string savedName = PlayerPrefs.GetString(PLAYER_NAME_PREF_KEY, "Player_" + Random.Range(100, 999));
            playerNameInput.text = savedName;
            PhotonNetwork.NickName = savedName;
            playerNameInput.onValueChanged.AddListener(OnPlayerNameChanged);
        }

        // Set up button listeners
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
        if (createLobbyButton != null) createLobbyButton.onClick.AddListener(ShowCreateLobbyPanel);
        if (joinWithCodeButton != null) joinWithCodeButton.onClick.AddListener(ShowJoinRoomPanel);
        if (quickJoinButton != null) quickJoinButton.onClick.AddListener(QuickJoin);

        if (joinLobbyButton != null)
        {
            joinLobbyButton.onClick.AddListener(JoinSelectedRoom);
            joinLobbyButton.interactable = false;
        }

        // Hide sub-panels by default
        if (createLobbyPanel != null) createLobbyPanel.gameObject.SetActive(false);
        if (joinRoomPanel != null) joinRoomPanel.gameObject.SetActive(false);

        // Connect to Photon
        ConnectToPhoton();
    }

    private void UpdateStatus(string message)
    {
        Debug.Log("[Lobby] " + message);
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    private void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            UpdateStatus("Connecting to Server...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (!PhotonNetwork.InLobby)
        {
            UpdateStatus("Joining Lobby...");
            PhotonNetwork.JoinLobby();
        }
        else
        {
            UpdateStatus("Connected & In Lobby");
        }
    }

    private void OnPlayerNameChanged(string newName)
    {
        if (string.IsNullOrEmpty(newName)) return;
        PhotonNetwork.NickName = newName;
        PlayerPrefs.SetString(PLAYER_NAME_PREF_KEY, newName);
        PlayerPrefs.Save();
    }

    private void GoToMainMenu()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        SceneManager.LoadScene("MainMenu");
    }

    private void ShowCreateLobbyPanel()
    {
        if (createLobbyPanel != null)
        {
            createLobbyPanel.gameObject.SetActive(true);
        }
    }

    private void ShowJoinRoomPanel()
    {
        if (joinRoomPanel != null)
        {
            joinRoomPanel.gameObject.SetActive(true);
        }
    }

    private void QuickJoin()
    {
        UpdateStatus("Quick Joining...");
        PhotonNetwork.JoinRandomRoom();
    }

    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        UpdateStatus("Connected to Master. Joining Lobby...");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        UpdateStatus("In Lobby. Ready to Play!");
        roomCache.Clear();
        ClearRoomListView();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateRoomCache(roomList);
        UpdateRoomListView();
    }

    public override void OnJoinedRoom()
    {
        UpdateStatus("Joined Room successfully!");
        if (PhotonNetwork.IsMasterClient)
        {
            UpdateStatus("Master Client: Loading Multiplayer Scene...");
            PhotonNetwork.LoadLevel("MultiplayerSence");
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        UpdateStatus("No public room found. Creating a new room...");
        string roomName = "Room_" + Random.Range(1000, 9999);
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        UpdateStatus("Create Room Failed: " + message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        UpdateStatus("Join Room Failed: " + message);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        UpdateStatus("Disconnected: " + cause.ToString());
    }

    #endregion

    #region Room List Rendering

    private void UpdateRoomCache(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList || room.PlayerCount == 0 || !room.IsOpen)
            {
                roomCache.Remove(room.Name);
            }
            else
            {
                roomCache[room.Name] = room;
            }
        }
    }

    private void SelectLobby(LobbyDetail lobbyDetail)
    {
        if (selectedLobbyDetail != null)
        {
            selectedLobbyDetail.SetSelected(false);
        }

        selectedLobbyDetail = lobbyDetail;

        if (selectedLobbyDetail != null)
        {
            selectedLobbyDetail.SetSelected(true);
            if (joinLobbyButton != null)
            {
                joinLobbyButton.interactable = true;
            }
        }
        else
        {
            if (joinLobbyButton != null)
            {
                joinLobbyButton.interactable = false;
            }
        }
    }

    private void JoinSelectedRoom()
    {
        if (selectedLobbyDetail != null)
        {
            JoinRoomByName(selectedLobbyDetail.RoomName);
        }
    }

    private void UpdateRoomListView()
    {
        ClearRoomListView();
        SelectLobby(null); // Clear selection when recreating list

        if (content == null || lobbyItemPrefab == null) return;

        foreach (var roomEntry in roomCache)
        {
            RoomInfo room = roomEntry.Value;
            LobbyDetail item = Instantiate(lobbyItemPrefab, content);
            if (item != null)
            {
                item.Setup(room, SelectLobby);
                roomListGameObjects.Add(item.gameObject);
            }
        }
    }

    private void ClearRoomListView()
    {
        foreach (GameObject go in roomListGameObjects)
        {
            if (go != null) Destroy(go);
        }
        roomListGameObjects.Clear();
    }

    private void JoinRoomByName(string roomName)
    {
        UpdateStatus("Joining room: " + roomName);
        PhotonNetwork.JoinRoom(roomName);
    }

    #endregion
}
