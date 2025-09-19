using UnityEngine;

public interface IPoolable
{
    void OnPoolSpawn();
    void OnPoolDespawn();

    void SetOriginalPrefab(GameObject prefab);
    GameObject GetOriginalPrefab();
}