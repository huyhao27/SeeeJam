using System.Collections.Generic;
using UnityEngine;

// Spawner luôn giữ đủ số lượng enemy quanh Player trong một bán kính.
// Không phụ thuộc SpawnZone. Dùng EnemySpawnManager + ObjectPool cho hiệu năng.
public class PlayerRadiusSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player; // nếu null, tự tìm theo tag "Player"
    [SerializeField] private BaseEnemy enemyPrefab; // dùng để tham số hoá API; thực tế lấy từ EnemyPool

    [Header("Counts & Radius")]
    [SerializeField] private int targetCount = 10;     // số enemy mong muốn quanh player
    [SerializeField] private float spawnRadius = 12f;  // bán kính spawn quanh player
    [SerializeField] private float safeRadius = 2.5f;  // không spawn quá gần player
    [SerializeField] private float despawnRadius = 20f;// nếu enemy đi quá xa, trả về pool

    [Header("Tick")]
    [SerializeField] private float tickInterval = 0.5f; // giãn cách kiểm tra
    [SerializeField] private int spawnPerTick = 3;      // giới hạn số spawn mỗi tick để tránh spike

    private readonly List<BaseEnemy> _active = new List<BaseEnemy>();
    private float _nextTick;
    
    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    private void Update()
    {
        if (Time.time < _nextTick) return;
        _nextTick = Time.time + tickInterval;
        Tick();
    }

    private void Tick()
    {
        if (player == null)
        {
            var po = GameObject.FindWithTag("Player");
            if (po == null) return;
            player = po.transform;
        }

        // Đảm bảo tham số hợp lệ để tránh spawn dày bất thường
        if (despawnRadius < spawnRadius)
        {
            despawnRadius = Mathf.Max(spawnRadius + 0.5f, spawnRadius);
        }
        if (safeRadius >= spawnRadius)
        {
            safeRadius = Mathf.Max(0f, spawnRadius * 0.3f);
        }

        // Dọn danh sách (null/inactive)
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var e = _active[i];
            if (e == null || !e.gameObject.activeSelf)
            {
                _active.RemoveAt(i);
            }
        }

        // Đếm số enemy còn sống trong phạm vi despawnRadius (alive)
        int alive = 0;
        for (int i = 0; i < _active.Count; i++)
        {
            var e = _active[i];
            if (e == null) continue;
            float d = Vector2.Distance(player.position, e.transform.position);
            if (d > despawnRadius)
            {
                // Quá xa -> despawn
                if (EnemySpawnManager.Instance != null)
                {
                    EnemySpawnManager.Instance.Despawn(e);
                }
                _active.RemoveAt(i);
                i--;
            }
            else
            {
                // vẫn nằm trong vùng theo dõi -> tính alive
                alive++;
            }
        }

        // Nếu số alive (<= despawnRadius) đã đủ, không spawn thêm
        int need = targetCount - alive;
        if (need <= 0 || EnemySpawnManager.Instance == null) return;
    int toSpawn = Mathf.Min(spawnPerTick, need);
        if (debugLog)
        {
            Debug.Log($"[PlayerRadiusSpawner] alive={alive}, need={need}, toSpawn={toSpawn}, activeList={_active.Count}");
        }
        for (int i = 0; i < toSpawn; i++)
        {
            Vector2 pos2D = Random.insideUnitCircle.normalized * Random.Range(safeRadius, spawnRadius);
            Vector3 spawnPos = player.position + new Vector3(pos2D.x, pos2D.y, 0f);
            var e = EnemySpawnManager.Instance.Spawn(enemyPrefab, spawnPos, Quaternion.identity);
            if (e != null)
            {
                _active.Add(e);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.2f);
        Gizmos.DrawSphere(player.position, spawnRadius);
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.2f);
        Gizmos.DrawSphere(player.position, safeRadius);
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.2f);
        Gizmos.DrawWireSphere(player.position, despawnRadius);
    }
}
