using UnityEngine;

public class WinUI : MonoBehaviour
{
    public static WinUI Instance;

    public GameObject winPanel;

    private void Awake()
    {
        Instance = this;

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    public void ShowWin()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }
}