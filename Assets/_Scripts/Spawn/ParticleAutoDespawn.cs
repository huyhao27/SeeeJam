using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleAutoDespawn : MonoBehaviour
{
    private ParticleSystem ps;

    private void OnEnable()
    {
        if (ps == null)
        {
            ps = GetComponent<ParticleSystem>();
        }
    }

    private void Update()
    {
        if (ps && !ps.IsAlive(true))
        {
            PoolManager.Instance.Despawn(gameObject);
        }
    }
}