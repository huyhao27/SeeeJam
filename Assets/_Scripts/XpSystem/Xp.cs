using UnityEngine;
using DG.Tweening;

public class Xp : MonoBehaviour
{
    private bool isCollected = false;

    public void Collect(Transform player)
    {
        if (isCollected) return;
        isCollected = true;

        // disable collider để không collect nhiều lần
        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // tween bay về player
        transform.DOMove(player.position, 0.4f)
            .SetEase(Ease.InQuad)
            .OnComplete(OnCollect);
    }

    private void OnCollect()
    {
        Debug.Log("XP collected!");
    }
}