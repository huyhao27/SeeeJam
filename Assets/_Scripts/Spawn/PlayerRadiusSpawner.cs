using System.Collections.Generic;
using UnityEngine;


public class PlayerRadiusSpawner : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private Transform player;

    [Header("Radius Settings")] [SerializeField]
    private float spawnRadius = 12f;

    [SerializeField] private float safeRadius = 2.5f;
    [SerializeField] private float despawnRadius = 20f;

    [Header("Debug")] [SerializeField] private bool debugLog = false;

    private readonly List<BaseEnemy> _activeEnemies = new List<BaseEnemy>();
    private Queue<BaseEnemy> _spawnQueue = new Queue<BaseEnemy>();
    private float _spawnTimer;
    private WaveData _currentWaveData;

    public bool IsSpawningFinished => _spawnQueue.Count == 0;

    private void Update()
    {
        if (player == null)
        {
            var po = GameObject.FindWithTag("Player");
            if (po == null) return;
            player = po.transform;
        }

        // Spawn theo hàng chờ
        if (!IsSpawningFinished)
        {
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnFromQueue();
            }
        }

        CleanupDistantEnemies();
    }

    // --- API ---
    public void Activate(WaveData waveData)
    {
        _currentWaveData = waveData;
        _spawnQueue.Clear();

        // 1. Tính toán số lượng cho mỗi loại
        int countA = Mathf.RoundToInt(waveData.totalEnemyCount * waveData.ratioTypeA);
        int countB = waveData.totalEnemyCount - countA;

        // 2. Tạo danh sách tạm
        var tempList = new List<BaseEnemy>();
        if (waveData.enemyTypeA != null)
            for (int i = 0; i < countA; i++)
                tempList.Add(waveData.enemyTypeA);

        if (waveData.enemyTypeB != null)
            for (int i = 0; i < countB; i++)
                tempList.Add(waveData.enemyTypeB);

        // 3. Xáo trộn (Fisher-Yates shuffle)
        for (int i = 0; i < tempList.Count - 1; i++)
        {
            int rnd = Random.Range(i, tempList.Count);
            (tempList[i], tempList[rnd]) = (tempList[rnd], tempList[i]);
        }

        // 4. Đưa vào hàng chờ
        _spawnQueue = new Queue<BaseEnemy>(tempList);
        _spawnTimer = _currentWaveData.spawnInterval;

        if (debugLog)
            Debug.Log($"[PlayerRadiusSpawner] Kích hoạt wave. Hàng chờ có {_spawnQueue.Count} quái.");
    }

    public void Deactivate()
    {
        _spawnQueue.Clear();

        // Dọn hết quái còn sống
        foreach (var e in _activeEnemies)
        {
            if (e != null && EnemySpawnManager.Instance != null)
                EnemySpawnManager.Instance.Despawn(e);
        }

        _activeEnemies.Clear();
    }

    private void SpawnFromQueue()
    {
        if (IsSpawningFinished || _currentWaveData == null) return;

        int amountToSpawn = Mathf.Min(_currentWaveData.spawnCountPerTick, _spawnQueue.Count);
        for (int i = 0; i < amountToSpawn; i++)
        {
            BaseEnemy prefab = _spawnQueue.Dequeue();
            Vector2 pos2D = Random.insideUnitCircle.normalized * Random.Range(safeRadius, spawnRadius);
            Vector3 spawnPos = player.position + new Vector3(pos2D.x, pos2D.y, 0f);

            var enemyInstance = EnemySpawnManager.Instance.Spawn(prefab, spawnPos, Quaternion.identity);
            if (enemyInstance != null) _activeEnemies.Add(enemyInstance);
        }

        _spawnTimer = _currentWaveData.spawnInterval;
    }

    private void CleanupDistantEnemies()
    {
        for (int i = _activeEnemies.Count - 1; i >= 0; i--)
        {
            var e = _activeEnemies[i];
            if (e == null || !e.gameObject.activeSelf)
            {
                _activeEnemies.RemoveAt(i);
                continue;
            }

            float d = Vector2.Distance(player.position, e.transform.position);
            if (d > despawnRadius && EnemySpawnManager.Instance != null)
            {
                EnemySpawnManager.Instance.Despawn(e);
                _activeEnemies.RemoveAt(i);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = player != null ? player.position : transform.position;

#if UNITY_EDITOR
        UnityEditor.Handles.color = new Color(0.2f, 0.8f, 1f, 0.2f);
        UnityEditor.Handles.DrawSolidDisc(center, Vector3.forward, spawnRadius);
        UnityEditor.Handles.color = new Color(1f, 0.3f, 0.3f, 0.2f);
        UnityEditor.Handles.DrawSolidDisc(center, Vector3.forward, safeRadius);
        UnityEditor.Handles.color = new Color(1f, 0.6f, 0f, 1f);
        UnityEditor.Handles.DrawWireDisc(center, Vector3.forward, despawnRadius);
#else
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.2f);
        Gizmos.DrawSphere(center, spawnRadius);
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.2f);
        Gizmos.DrawSphere(center, safeRadius);
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.2f);
        Gizmos.DrawWireSphere(center, despawnRadius);
#endif
    }
}