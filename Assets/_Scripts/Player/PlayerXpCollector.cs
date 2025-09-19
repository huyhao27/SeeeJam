using UnityEngine;

public class PlayerXpCollector : MonoBehaviour
{
    public float pickupRange = 3f;
    public LayerMask xpLayer; 

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