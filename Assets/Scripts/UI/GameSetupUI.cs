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
    [SerializeField] private Button backToLobbyButton;

}
