using System.Collections.Generic;
using UnityEngine;

// Hệ thống Spawn/Pool Enemy
// Spawn theo yêu cầu từ các SpawnZone
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
        enemy.OnSpawn();
        return enemy;
    }

    public void Despawn(BaseEnemy instance)
    {
        if (instance == null) return;
        instance.OnDespawn();
        ObjectPool<BaseEnemy>.Instance.Return(instance);
    }

    // API spawn theo vị trí ngẫu nhiên trong vùng (hình chữ nhật phẳng XZ)
    public BaseEnemy SpawnInArea(BaseEnemy prefab, Bounds area)
    {
        var pos = new Vector3(
            Random.Range(area.min.x, area.max.x),
            0f,
            Random.Range(area.min.z, area.max.z)
        );
        return Spawn(prefab, pos, Quaternion.identity);
    }
}
