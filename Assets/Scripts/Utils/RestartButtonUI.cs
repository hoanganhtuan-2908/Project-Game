using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartButtonUI : MonoBehaviour
{
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;

        // Play main menu BGM
        if (FMODAudioManager.Instance != null)
        {
            FMODAudioManager.Instance.PlayMenuTheme();
        }

        SceneManager.LoadScene("MainMenu");
    }
}