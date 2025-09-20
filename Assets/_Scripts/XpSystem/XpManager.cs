using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XpManager : Singleton<XpManager>
{
    [SerializeField] private List<Xp> xpPrefabs;

    private Transform player;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 15f;      // bán kính spawn quanh player
    [SerializeField] private float spawnInterval = 2f;     // thời gian giữa các lần spawn
    [SerializeField] private int spawnPerWave = 5;         // số lượng spawn mỗi wave
    [SerializeField] private int maxXpOnMap = 200;         // giới hạn số XP trên map

    [Header("Despawn Settings")]
    [SerializeField] private float despawnDistance = 40f;  // khoảng cách quá xa thì despawn

    private int currentXpCount = 0;

    void Start()
    {
        player = PlayerStats.Instance.gameObject.transform;
        StartCoroutine(SpawnLoop());
    }

    /// <summary>
    /// Vòng lặp spawn XP theo thời gian
    /// </summary>
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (player != null && currentXpCount < maxXpOnMap)
            {
                SpawnXpNearPlayer(1, spawnPerWave); // mặc định level 1, bạn có thể chỉnh sau
            }

            DespawnFarXp();
        }
    }

    /// <summary>
    /// Spawn XP quanh player
    /// </summary>
    public void SpawnXpNearPlayer(int level, int count)
    {
        int index = Mathf.Clamp(level - 1, 0, xpPrefabs.Count - 1);

        for (int i = 0; i < count; i++)
        {
            if (currentXpCount >= maxXpOnMap) return;

            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = player.position + new Vector3(randomCircle.x, randomCircle.y, 0f);

            var xp = PoolManager.Instance.Spawn<Xp>(xpPrefabs[index]);
            xp.transform.position = spawnPos;

            currentXpCount++;
        }
    }

    /// <summary>
    /// Despawn XP nào quá xa player
    /// </summary>
    private void DespawnFarXp()
    {
        if (player == null) return;

        var allXp = FindObjectsOfType<Xp>();
        foreach (var xp in allXp)
        {
            if (Vector3.Distance(player.position, xp.transform.position) > despawnDistance)
            {
                ReleaseXp(xp.gameObject);
            }
        }
    }

    public void GetXp(int level)
    {
        int index = level - 1;
        PoolManager.Instance.Spawn<Xp>(xpPrefabs[index]);
    }

    /// <summary>
    /// Spawn một XP prefab cấp (level) tại vị trí xác định, gán amount tuỳ chỉnh.
    /// </summary>
    public Xp SpawnXpAt(int level, Vector3 position, float amount)
    {
        int index = Mathf.Clamp(level - 1, 0, xpPrefabs.Count - 1);
        var xp = PoolManager.Instance.Spawn<Xp>(xpPrefabs[index]);
        xp.transform.position = position;
        xp.SetAmount(amount);
        currentXpCount++;
        return xp;
    }

    public void ReleaseXp(GameObject xp)
    {
        currentXpCount--;
        PoolManager.Instance.Despawn(xp);
    }


}
