using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class JoinRoomUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField roomIDInput;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private Button closeButton;

    private void Start()
    {
        if (joinRoomButton != null) joinRoomButton.onClick.AddListener(JoinRoom);
        if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
    }

    private void JoinRoom()
    {
        if (roomIDInput == null) return;

        string roomID = roomIDInput.text;
        if (string.IsNullOrEmpty(roomID))
        {
            Debug.LogWarning("[JoinRoomUI] Room ID is empty!");
            return;
        }

        Debug.Log($"[JoinRoomUI] Joining room: {roomID}");
        PhotonNetwork.JoinRoom(roomID);
        ClosePanel();
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
