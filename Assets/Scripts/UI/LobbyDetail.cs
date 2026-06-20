using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

public class LobbyDetail : MonoBehaviour
{
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image selectionHighlight; // Optional: Image to highlight selection

    public string RoomName { get; private set; }
    private System.Action<LobbyDetail> selectCallback;

    private void Start()
    {
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnSelectClicked);
        }

        // Initially unselected
        SetSelected(false);
    }

    public void Setup(RoomInfo roomInfo, System.Action<LobbyDetail> onSelected)
    {
        RoomName = roomInfo.Name;
        selectCallback = onSelected;

        if (roomNameText != null) roomNameText.text = RoomName;
        if (playerCountText != null) playerCountText.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
        if (statusText != null)
        {
            statusText.text = roomInfo.PlayerCount >= roomInfo.MaxPlayers ? "Full" : "Waiting...";
        }

        // If the room is full, make it unselectable (optional, or just handle in join button)
        if (selectButton != null)
        {
            selectButton.interactable = roomInfo.PlayerCount < roomInfo.MaxPlayers;
        }
    }

    private void OnSelectClicked()
    {
        selectCallback?.Invoke(this);
    }

    public void SetSelected(bool isSelected)
    {
        if (selectionHighlight != null)
        {
            selectionHighlight.gameObject.SetActive(isSelected);
        }
    }
}
