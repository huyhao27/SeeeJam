using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private GameObject _prefab;
    private readonly Queue<GameObject> _pool = new Queue<GameObject>();

    public GameObject Prefab => _prefab; // expose for fallback assignment

    public void Configure(GameObject prefab, int initialSize)
    {
        _prefab = prefab;
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = CreateNewObject();
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    private GameObject CreateNewObject()
    {
        GameObject newObj = Instantiate(_prefab, transform);
        
        var poolable = newObj.GetComponent<IPoolable>();
        if (poolable != null)
        {
            poolable.SetOriginalPrefab(_prefab);
        }
        return newObj;
    }

    public GameObject Get()
    {
        if (_pool.Count == 0)
        {
            GameObject newObj = CreateNewObject();
            _pool.Enqueue(newObj);
        }

        GameObject obj = _pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform); 
        _pool.Enqueue(obj);
    }
}