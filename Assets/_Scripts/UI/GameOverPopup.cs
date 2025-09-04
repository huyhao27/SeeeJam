using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPopup : Popup
{
    [Tooltip("Name scene")]
    [SerializeField] private string mainMenuSceneName = "SampleScene";

    public void OnRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName); 
    }
}