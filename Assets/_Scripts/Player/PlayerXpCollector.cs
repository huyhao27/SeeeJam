using UnityEngine;

public class PlayerXpCollector : MonoBehaviour
{
    private float pickupRange => PlayerStats.Instance != null ? PlayerStats.Instance.CollectRadius : 1f;
    public LayerMask xpLayer;

    private PlayerStats playerStats;

    void Start()
    {

        playerStats = GetComponent<PlayerStats>();
    }
    void Update()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRange, xpLayer);

        foreach (var hit in hits)
        {
            Xp xp = hit.GetComponent<Xp>();
            if (xp != null)
            {
                xp.Collect(transform); 
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}