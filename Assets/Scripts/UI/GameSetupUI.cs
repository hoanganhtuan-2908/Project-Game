using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSetupUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName1;
    [SerializeField] private TextMeshProUGUI playerName2;
    [SerializeField] private TextMeshProUGUI roomName;
    [SerializeField] private TextMeshProUGUI roomID;
    [SerializeField] private TMP_Dropdown chooseBlackOrWhiteDropdown1;
    [SerializeField] private TMP_Dropdown chooseBlackOrWhiteDropdown2;
    [SerializeField] private TMP_Dropdown chooseSkinDropdown;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button backToLobbyButton;

    [Header("Skins")]
    [SerializeField] private List<ChessSkin> availableSkins;

    public TextMeshProUGUI PlayerName1 => playerName1;
    public TextMeshProUGUI PlayerName2 => playerName2;
    public TextMeshProUGUI RoomName => roomName;
    public TextMeshProUGUI RoomID => roomID;
    public TMP_Dropdown ChooseBlackOrWhiteDropdown1 => chooseBlackOrWhiteDropdown1;
    public TMP_Dropdown ChooseBlackOrWhiteDropdown2 => chooseBlackOrWhiteDropdown2;
    public TMP_Dropdown ChooseSkinDropdown => chooseSkinDropdown;
    public Button StartGameButton => startGameButton;
    public Button ReadyButton => readyButton;
    public Button BackToLobbyButton => backToLobbyButton;
    public List<ChessSkin> AvailableSkins => availableSkins;
}
