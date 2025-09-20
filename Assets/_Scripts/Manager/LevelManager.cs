using UnityEngine;
using System.Collections.Generic;

public class LevelManager : Singleton<LevelManager>
{
    [Header("Level Configuration")]
    [SerializeField] private LevelData currentLevelData;

    [Header("Dependencies")]
    [SerializeField] private PlayerRadiusSpawner radiusSpawner;


    // --- Trạng thái màn chơi ---
    private float levelTimer;
    private int enemiesKilled = 0;
    private int currentWaveIndex = -1;
    private bool bossSpawned = false;
    private int _activeEnemyCount = 0;



    protected override void Awake()
    {
        base.Awake();
        if (radiusSpawner == null)
        {
            radiusSpawner = FindObjectOfType<PlayerRadiusSpawner>();
            if (radiusSpawner == null) Debug.LogError("LevelManager không tìm thấy PlayerRadiusSpawner trong Scene!");
        }
    }

    private void Start()
    {
        InitializeLevel();
    }

    private void OnEnable()
    {
        EventBus.On(GameEvent.EnemyDied, OnEnemyDied);
        EventBus.On(GameEvent.EnemySpawned, OnEnemySpawned);
    }

    private void OnDisable()
    {
        EventBus.Off(GameEvent.EnemyDied, OnEnemyDied);
        EventBus.Off(GameEvent.EnemySpawned, OnEnemySpawned);
        
        if (radiusSpawner != null)
        {
            radiusSpawner.Deactivate();
        }
    }

    private void InitializeLevel()
    {
        if (currentLevelData == null)
        {
            Debug.LogError("LevelManager: currentLevelData chưa được gán!");
            this.enabled = false;
            return;
        }

        levelTimer = currentLevelData.levelDuration;
        enemiesKilled = 0;
        currentWaveIndex = -1;
        bossSpawned = false;
        _activeEnemyCount = 0;
        
        radiusSpawner.Deactivate();
        
        Debug.Log("Level Initialized. Duration: " + levelTimer + "s");
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        levelTimer -= Time.deltaTime;
        EventBus.Emit(GameEvent.TimerUpdated, levelTimer);
        
        if (levelTimer <= 0 && !bossSpawned)
        {
            levelTimer = 0;
            GameManager.Instance.ChangeState(GameState.GameOver);
            Debug.Log("Game Over - Time's up!");
            this.enabled = false;
            return;
        }

        CheckForNextWave();
        CheckForBossSpawnCondition();
    }

    private void CheckForNextWave()
    {
        int nextWaveIndex = currentWaveIndex + 1;
        if (nextWaveIndex < currentLevelData.waves.Count)
        {
            // Tính thời điểm bắt đầu wave tiếp theo (tính theo thời gian còn lại)
            float waveStartTimeThreshold = currentLevelData.levelDuration * (1f - currentLevelData.waves[nextWaveIndex].startTimePercentage);
            if (levelTimer <= waveStartTimeThreshold)
            {
                StartWave(nextWaveIndex);
            }
        }
    }

    private void StartWave(int waveIndex)
    {
        currentWaveIndex = waveIndex;
        WaveData waveData = currentLevelData.waves[waveIndex];
        
        EventBus.Emit(GameEvent.WaveStarted, waveIndex + 1);

        radiusSpawner.Activate(waveData);
    }

    private void CheckForBossSpawnCondition()
    {
        if (bossSpawned) return;

        bool isLastWave = currentWaveIndex == currentLevelData.waves.Count - 1;
        if (isLastWave && radiusSpawner.IsSpawningFinished && _activeEnemyCount == 0)
        {
            SpawnBoss();
        }
    }

    private void SpawnBoss()
    {
        bossSpawned = true;
        Debug.Log("Spawning BOSS!");
        Vector3 pos = currentLevelData.bossSpawnPoint != null ? currentLevelData.bossSpawnPoint.position : Vector3.zero;
        EnemySpawnManager.Instance.Spawn(currentLevelData.bossPrefab, pos, Quaternion.identity);
    }
    

    private void OnEnemySpawned(object data)
    {
        _activeEnemyCount++;
    }

    private void OnEnemyDied(object data)
    {
        if (_activeEnemyCount > 0) _activeEnemyCount--;
        enemiesKilled++;
        EventBus.Emit(GameEvent.KillCountUpdated, enemiesKilled);

        if (data is BaseEnemy enemy && enemy.CompareTag("Boss"))
        {
            EventBus.Emit(GameEvent.Win, true); // true = win
            GameManager.Instance.ChangeState(GameState.Win);
            Debug.Log("YOU WIN!");
        }
    }
}