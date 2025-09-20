using UnityEngine;

public class Note : MonoBehaviour
{
    [SerializeField] private int type;

    public void SetType(int newType)
    {
        this.type = newType;
    }

    public void onHit()
    {
        SoundManager.Instance.PlaySfx(this.type + 1);
        EventBus.Emit(GameEvent.ActivateSkill, this.type);
        Debug.Log("Note hit! Type: " + type);
    }
}