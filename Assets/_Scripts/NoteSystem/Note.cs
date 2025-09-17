using UnityEngine;

public class Note : MonoBehaviour
{
    [SerializeField] private int type;

    public void onHit()
    {
        EventBus.Emit(GameEvent.ActivateSkill, this.type);
    }
}