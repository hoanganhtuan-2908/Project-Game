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
            winPanel.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogError("[WinUI] winPanel is not assigned.");
        }

        if (restartPanel != null)
        {
            restartPanel.SetActive(true);
            restartPanel.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogError("[WinUI] restartPanel is not assigned.");
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
