using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPopup : Popup
{
    [Tooltip("Name scene")]
    [SerializeField] private string mainMenuSceneName = "SampleScene";

    public void OnRetry()
    {
        EventBus.ClearAll();
        // Load lại scene hiện tại
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        // Reset time scale (phòng khi game bị pause)
        Time.timeScale = 1f;
    }

    public void OnMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}