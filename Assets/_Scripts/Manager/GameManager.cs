using System;
using UnityEngine;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}

public class GameManager : Singleton<GameManager>
{
    public static event Action<GameState> OnGameStateChanged;

    public GameState CurrentState { get; private set; }
    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        
        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f; 
                break;
            case GameState.GameOver:
                Time.timeScale = 1f; // or 0f
                break;
        }

        OnGameStateChanged?.Invoke(newState);
        Debug.Log("Game state changed to: " + newState);
    }
}