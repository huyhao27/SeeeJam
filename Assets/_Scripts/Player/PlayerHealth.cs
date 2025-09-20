using UnityEngine;

public class PlayerHealth : MonoBehaviour, IAffectable
{
    private float _currentHp;
    private float _maxHp;
    private Effectable _effectable;

    private void Awake()
    {
        _effectable = GetComponent<Effectable>() ?? gameObject.AddComponent<Effectable>();
    }

    private void Start()
    {
        _maxHp = PlayerStats.Instance.MaxHp;
        _currentHp = _maxHp;
        
        EventBus.Emit(GameEvent.PlayerHealthUpdated, new object[] { _currentHp, _maxHp });
    }

    private void OnEnable()
    {
        EventBus.On(GameEvent.PlayerDamaged, OnTakeDamageEvent);
    }

    private void OnDisable()
    {
        EventBus.Off(GameEvent.PlayerDamaged, OnTakeDamageEvent);
    }

    public void TakeDamage(int damage)
    {
        if (_currentHp <= 0) return;

        _currentHp -= damage;
        if (_currentHp <= 0)
        {
            _currentHp = 0;
            Die();
        }
        
        EventBus.Emit(GameEvent.PlayerHealthUpdated, new object[] { _currentHp, _maxHp });
    }

    private void OnTakeDamageEvent(object data)
    {
        if (data is object[] args && args.Length > 0 && args[0] is int damage)
        {
            TakeDamage(damage);
        }
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        GameManager.Instance.ChangeState(GameState.GameOver);
    }
    
    public void AddEffect(IEffect effect)
    {
        _effectable.AddEffect(effect);
    }
}