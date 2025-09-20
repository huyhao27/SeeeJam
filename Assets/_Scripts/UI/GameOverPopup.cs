using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPopup : Popup
{

    public void OnRetry()
    {
        EventBus.ClearAll();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        Time.timeScale = 1f;
    }


    public void OnQuit()
    {
        Debug.Log("Quitting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // Nếu đang chạy bản build game, đóng ứng dụng
            Application.Quit();
#endif
    }
}