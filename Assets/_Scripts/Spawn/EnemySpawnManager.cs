using System.Collections.Generic;
using UnityEngine;

// Hệ thống Spawn/Pool Enemy (2D)
// Có thể được gọi bởi PlayerRadiusSpawner (spawn theo bán kính quanh Player)
public class EnemySpawnManager : MonoBehaviour
{
    public static EnemySpawnManager Instance { get; private set; }

    [Header("Cài đặt chung")]
    [SerializeField] private Transform globalContainer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // nếu muốn giữ qua scene
    }

    // Spawn sử dụng pool nếu có
    public BaseEnemy Spawn(BaseEnemy prefab, Vector3 position, Quaternion rotation)
    {
        var enemy = ObjectPool<BaseEnemy>.Instance.Get();
        if (enemy == null)
        {
            Debug.LogWarning("Spawn thất bại: ObjectPool<BaseEnemy> chưa cấu hình objectToPool");
            return null;
        }
        enemy.transform.SetPositionAndRotation(position, rotation);
        if (globalContainer != null) enemy.transform.SetParent(globalContainer, true);
        enemy.gameObject.SetActive(true);
        enemy.SetPooledManaged(true);
        enemy.OnPersistentSpawn();
        return enemy;
    }

    public void Despawn(BaseEnemy instance)
    {
        if (instance == null) return;
        instance.OnPersistentDespawn();
        ObjectPool<BaseEnemy>.Instance.Return(instance);
    }

    // API spawn theo vị trí ngẫu nhiên trong vùng 2D (hình chữ nhật trên mặt phẳng XY)
    public BaseEnemy SpawnInArea(BaseEnemy prefab, Bounds area)
    {
        var pos = new Vector3(
            Random.Range(area.min.x, area.max.x),
            Random.Range(area.min.y, area.max.y),
            0f
        );
        return Spawn(prefab, pos, Quaternion.identity);
    }
}