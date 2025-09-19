using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

[System.Serializable]
public class PoolInfo
{
    public MonoBehaviour prefab;
    public int initialSize = 10;
}

public class PoolManager : Singleton<PoolManager>
{
    [SerializeField] private List<PoolInfo> poolsToCreate;

    private readonly Dictionary<string, ObjectPool> _poolDictionary = new Dictionary<string, ObjectPool>();

    protected override void Awake()
    {
        base.Awake();
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var poolInfo in poolsToCreate)
        {
            if (poolInfo.prefab == null) continue;
            if (_poolDictionary.ContainsKey(poolInfo.prefab.name)) continue;

            GameObject poolContainer = new GameObject($"{poolInfo.prefab.name}_Pool");
            poolContainer.transform.SetParent(this.transform);

            var poolComponent = poolContainer.AddComponent<ObjectPool>();
            poolComponent.Configure(poolInfo.prefab.gameObject, poolInfo.initialSize);

            _poolDictionary.Add(poolInfo.prefab.name, poolComponent);
        }
    }

    public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : MonoBehaviour, IPoolable
    {
        if (!_poolDictionary.ContainsKey(prefab.name))
        {
            Debug.LogError($"PoolManager không chứa pool cho prefab: {prefab.name}");
            return null;
        }

        ObjectPool pool = _poolDictionary[prefab.name];
        GameObject instanceGo = pool.Get();

        T instance = instanceGo.GetComponent<T>();

        instance.transform.SetPositionAndRotation(position, rotation);
        instance.OnPoolSpawn();

        return instance;
    }

    public T Spawn<T>(T prefab) where T : MonoBehaviour
    {
        if (!_poolDictionary.ContainsKey(prefab.name))
        {
            Debug.LogError($"PoolManager không chứa pool cho prefab: {prefab.name}");
            return null;
        }

        ObjectPool pool = _poolDictionary[prefab.name];
        GameObject instanceGo = pool.Get();
        return instanceGo.GetComponent<T>();
    }

    public void Despawn(IPoolable instance)
    {
        var instanceGo = ((MonoBehaviour)instance).gameObject;
        var prefab = instance.GetOriginalPrefab();

        if (prefab != null && _poolDictionary.ContainsKey(prefab.name))
        {
            instance.OnPoolDespawn();
            ObjectPool pool = _poolDictionary[prefab.name];
            pool.Return(instanceGo);
        }
        else
        {
            Debug.LogWarning($"Không tìm thấy pool cho {instanceGo.name}, sẽ Destroy.");
            Destroy(instanceGo);
        }
    }


    public void Despawn(GameObject prefab)
    {
        try
        {
            // Loại bỏ (Clone), (1), (2), ...
            string cleanName = Regex.Replace(prefab.name, @"\(\d*\)|\(Clone\)", "").Trim();

            ObjectPool pool = _poolDictionary[cleanName];
            pool.Return(prefab);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}