using UnityEngine;
using UnityEngine.SceneManagement;

public class PausePopup : Popup
{
    public void OnResume()
    {

        GameManager.Instance.ChangeState(GameState.Playing);
    }


    public void OnPlayAgain()
    {
        GameManager.Instance.RestartGame();
    }

    public void OnQuit()
    {
        Debug.Log("Quitting game...");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}