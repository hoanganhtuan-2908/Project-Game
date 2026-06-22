using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class CreateLobbyUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private Button createPublicLobbyButton;
    [SerializeField] private Button createPrivateLobbyButton;
    [SerializeField] private Button closeButton;

    private void Start()
    {
        if (createPublicLobbyButton != null) createPublicLobbyButton.onClick.AddListener(CreatePublicLobby);
        if (createPrivateLobbyButton != null) createPrivateLobbyButton.onClick.AddListener(CreatePrivateLobby);
        if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
    }

    private void CreatePublicLobby()
    {
        CreateLobby(true);
    }

    private void CreatePrivateLobby()
    {
        CreateLobby(false);
    }

    private void CreateLobby(bool isPublic)
    {
        string lobbyName = lobbyNameInput != null ? lobbyNameInput.text : "";
        if (string.IsNullOrEmpty(lobbyName))
        {
            lobbyName = "Room_" + Random.Range(1000, 9999);
        }

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = isPublic,
            IsOpen = true
        };

        Debug.Log($"[CreateLobby] Creating room: {lobbyName} (Public: {isPublic})");
        PhotonNetwork.CreateRoom(lobbyName, roomOptions);
        ClosePanel();
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
