using UnityEngine;

public class LossUI : MonoBehaviour
{
    public static LossUI Instance;

    [SerializeField] private GameObject lossPanel;
    [SerializeField] private GameObject restartPanel;

    private void Awake()
    {
        Instance = this;

        if (lossPanel != null)
            lossPanel.SetActive(false);

        if (restartPanel != null)
            restartPanel.SetActive(false);
    }

    public void ShowLoss()
    {
        Debug.Log("YOU LOSS");

        if (lossPanel != null)
            lossPanel.SetActive(true);

        if (restartPanel != null)
            restartPanel.SetActive(true);
    }

    public void HideLoss()
    {
        if (lossPanel != null)
            lossPanel.SetActive(false);

        if (restartPanel != null)
            restartPanel.SetActive(false);
    }
}