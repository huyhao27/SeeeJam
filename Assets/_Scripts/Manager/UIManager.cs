using UnityEngine;

public class UIManager : MonoBehaviour
{
    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void ShowGameOverScreen()
    {
        Debug.Log("Showing Game Over screen on UI!");
        // Todo: Implement UI logic to show game over screen
    }
    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.Paused)
        {
            
        }
        else if (state == GameState.GameOver)
        {
            
        }
        // ---------------------------------------- 
        else
        {
            // Handle other states if necessary
        }
    }
}