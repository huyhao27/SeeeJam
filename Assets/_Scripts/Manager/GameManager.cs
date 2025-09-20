
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver,
    Win,
    Tutorial,
}

public class GameManager : Singleton<GameManager>
{
    public static event Action<GameState> OnGameStateChanged;

    public GameState CurrentState { get; private set; }

    private void Start()
    {
        ChangeState(GameState.Tutorial);
        // ChangeState(GameState.Playing);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            RestartGame();
        }
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;

        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                PopupManager.Instance.HideAllPopups();
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                PopupManager.Instance.HideAllPopups();
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                PopupManager.Instance.ShowPopup<PausePopup>();
                break;
            case GameState.GameOver:
                Time.timeScale = 0f; // or 0f 
                SoundManager.Instance.PlaySfxOneShot(8);
                PopupManager.Instance.ShowPopup<GameOverPopup>();
                EventBus.Emit(GameEvent.GameOver, 0);
                break;
            case GameState.Tutorial:
                Time.timeScale = 0f;
                PopupManager.Instance.ShowPopup<Tutorial>();
                break;
            case GameState.Win:
                Time.timeScale = 0f;
                PopupManager.Instance.ShowPopup<WinPopup>();
                break;
        }

        OnGameStateChanged?.Invoke(newState);
        Debug.Log("Game state changed to: " + newState);
    }

    public void RestartGame()
    {
        EventBus.ClearAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        Time.timeScale = 1f;
    }
}

public enum GameEvent
{
    Win,
    Lose,
    GameOver,
    ActivateSkill,
    EnemyDied,
    KillCountUpdated,
    EnemySpawned,
    TimerUpdated,
    WaveStarted,

    GetXp,
    PlayerDamaged,
    PlayerHealthUpdated,

    SelectUpgrade,

    LevelUp,
    BossSpawned,
    BossHpChanged,
    BossSkillCast, // payload: skillId (int), maybe extra info
    BossDied,
    BossChargeStart,
    PlayerStunned, // payload: duration (float)

    Heal,

    MaxHpChanged,

    NoteCountChanged,
}