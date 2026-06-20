using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

public class LobbyListUI : MonoBehaviour
{
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text statusText;

    private string roomName;
    private System.Action<string> joinCallback;

    private void Start()
    {
        if (joinLobbyButton != null)
        {
            joinLobbyButton.onClick.AddListener(OnJoinClicked);
        }
    }

    public void Setup(RoomInfo roomInfo, System.Action<string> onJoinClicked)
    {
        roomName = roomInfo.Name;
        joinCallback = onJoinClicked;

        if (roomNameText != null) roomNameText.text = roomName;
        if (playerCountText != null) playerCountText.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
        if (statusText != null)
        {
            statusText.text = roomInfo.PlayerCount >= roomInfo.MaxPlayers ? "Full" : "Waiting...";
        }
        if (joinLobbyButton != null)
        {
            joinLobbyButton.interactable = roomInfo.PlayerCount < roomInfo.MaxPlayers;
        }
    }

    private void OnJoinClicked()
    {
        joinCallback?.Invoke(roomName);
    }
}
