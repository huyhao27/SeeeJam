using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPopup : Popup
{

    public void OnRetry()
    {
        GameManager.Instance.RestartGame();
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