using UnityEngine;
using DG.Tweening;

public class Xp : MonoBehaviour
{
    [SerializeField] private float amount;
    private bool isCollected = false;

    public void Collect(Transform player)
    {
        if (isCollected) return;
        isCollected = true;

        // disable collider để không collect nhiều lần
        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // gắn làm child của player
        transform.SetParent(player);

        // tween bay về local (0,0,0) = đúng tâm player
        transform.DOLocalMove(Vector3.zero, 0.15f)
            .SetEase(Ease.InQuad)
            .OnComplete(OnCollect);
    }

    private void OnCollect()
    {
        EventBus.Emit(GameEvent.GetXp, amount);
        XpManager.Instance.ReleaseXp(this.gameObject);
    }
}