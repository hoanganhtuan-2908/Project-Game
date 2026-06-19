using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private Transform content;
    [SerializeField] private LobbyListUI lobbyItemPrefab;

}
