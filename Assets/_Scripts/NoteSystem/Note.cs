using UnityEngine;

public class Note : MonoBehaviour
{
    [SerializeField] private int type;

    public void onHit()
    {
        EventBus.Emit(GameEvent.OnHitNote, this.type);
    }
}