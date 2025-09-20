using UnityEngine;
using TMPro; // <<< QUAN TRỌNG: Thêm dòng này để dùng TextMeshPro

public class LevelHUD : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI killCountText;
    [SerializeField] private TextMeshProUGUI waveText;

    private void OnEnable()
    {
        EventBus.On(GameEvent.TimerUpdated, OnTimerUpdate);
        EventBus.On(GameEvent.KillCountUpdated, OnKillsUpdate);
        EventBus.On(GameEvent.WaveStarted, OnWaveUpdate);
    }

    private void OnDisable()
    {
        EventBus.Off(GameEvent.TimerUpdated, OnTimerUpdate);
        EventBus.Off(GameEvent.KillCountUpdated, OnKillsUpdate);
        EventBus.Off(GameEvent.WaveStarted, OnWaveUpdate);
    }


    private void OnTimerUpdate(object data)
    {
        if (data is float timeRemaining)
        {
            timeRemaining = Mathf.Max(0, timeRemaining);
            
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    private void OnKillsUpdate(object data)
    {
        if (data is int killCount)
        {
            killCountText.text = $"Kills: {killCount}";
        }
    }

    private void OnWaveUpdate(object data)
    {
        if (data is int waveNumber)
        {
            waveText.text = $"Wave: {waveNumber}" + "/10";
        }
    }
}