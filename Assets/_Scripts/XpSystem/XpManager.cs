using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XpManager : Singleton<XpManager>
{
    [SerializeField] private List<Xp> xpPrefabs;

    void Start()
    {

    }

    public void GetXp(int level)
    {
        int index = level - 1;
        PoolManager.Instance.Spawn<Xp>(xpPrefabs[index]);
    }

    public void ReleaseXp(GameObject xp)
    {
        PoolManager.Instance.Despawn(xp);
    }
    
    
}
