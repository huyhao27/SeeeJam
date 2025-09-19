using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    public static EnemySpawnManager Instance { get; private set; }

    [Header("Cài đặt")]
    [Tooltip("Container để chứa tất cả enemy được spawn ra, giúp Hierarchy gọn gàng")]
    [SerializeField] private Transform globalContainer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public BaseEnemy Spawn(BaseEnemy prefab, Vector3 position, Quaternion rotation)
    {

        var enemy = PoolManager.Instance.Spawn(prefab, position, rotation);
        if (enemy == null)
        {
            Debug.LogWarning($"Spawn thất bại: PoolManager chưa được cấu hình cho prefab {prefab.name}");
            return null;
        }

        if (globalContainer != null)
        {
            enemy.transform.SetParent(globalContainer, true);
        }

        EventBus.Emit(GameEvent.EnemySpawned, enemy);
        
        return enemy;
    }


    public void Despawn(BaseEnemy instance)
    {
        if (instance == null) return;

        PoolManager.Instance.Despawn(instance);
    }
}