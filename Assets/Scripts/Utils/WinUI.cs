using UnityEngine;

public class WinUI : MonoBehaviour
{
    public static WinUI Instance;

    public GameObject winPanel;
    public GameObject restartPanel;

    private void Awake()
    {
        Instance = this;

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        if (restartPanel != null)
        {
            restartPanel.SetActive(false);
        }
    }

    public void ShowWin()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        if (restartPanel != null)
        {
            restartPanel.SetActive(true);
        }
    }
    public void HideWin()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        if (restartPanel != null)
        {
            restartPanel.SetActive(false);
        }
    }

}