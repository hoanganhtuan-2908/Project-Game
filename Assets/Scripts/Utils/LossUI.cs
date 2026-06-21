using UnityEngine;

public class LossUI : MonoBehaviour
{
    public static LossUI Instance;

    [SerializeField] private GameObject lossPanel;
    [SerializeField] private GameObject restartPanel;

    private void Awake()
    {
        bool isAttachedToOwnPanel = lossPanel == gameObject;

        if (!isAttachedToOwnPanel || Instance == null)
        {
            Instance = this;
        }

        if (isAttachedToOwnPanel)
        {
            return;
        }

        if (lossPanel != null)
            lossPanel.SetActive(false);

        if (restartPanel != null)
            restartPanel.SetActive(false);
    }

    public void ShowLoss()
    {
        Debug.Log("YOU LOSE");

        if (lossPanel != null)
        {
            lossPanel.SetActive(true);
            lossPanel.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogError("[LossUI] lossPanel is not assigned.");
        }

        if (restartPanel != null)
        {
            restartPanel.SetActive(true);
            restartPanel.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogError("[LossUI] restartPanel is not assigned.");
        }
    }

    public void HideLoss()
    {
        if (lossPanel != null)
            lossPanel.SetActive(false);

        if (restartPanel != null)
            restartPanel.SetActive(false);
    }
}
