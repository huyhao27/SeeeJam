using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> : Singleton<ObjectPool<T>> where T : MonoBehaviour
{
    [SerializeField] private T objectToPool;
    [SerializeField] private int initialPoolSize = 20;

    private Queue<T> pool = new Queue<T>();

    protected override void Awake()
    {
        base.Awake();

        if (Instance == this)
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                T obj = Instantiate(objectToPool);
                obj.gameObject.SetActive(false);
                pool.Enqueue(obj);
            }
        }
    }
    
    public T Get()
    {
        if (pool.Count > 0)
        {
            T obj = pool.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }

        T newObj = Instantiate(objectToPool);
        newObj.gameObject.SetActive(true);
        return newObj;
    }
    
    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}