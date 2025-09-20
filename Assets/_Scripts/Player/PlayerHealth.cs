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
        EventBus.On(GameEvent.MaxHpChanged, OnMaxHpChanged);
        EventBus.On(GameEvent.Heal, OnHeal);
    }

    private void OnDisable()
    {
        EventBus.Off(GameEvent.PlayerDamaged, OnTakeDamageEvent);
        EventBus.Off(GameEvent.MaxHpChanged, OnMaxHpChanged);
        EventBus.Off(GameEvent.Heal, OnHeal);
    }

    private void OnMaxHpChanged(object data)
    {
        if (data is float newMaxHp)
        {
            _maxHp = newMaxHp;
            EventBus.Emit(GameEvent.PlayerHealthUpdated, new object[] { _currentHp, _maxHp });
        }
    }
    
    private void OnHeal(object data)
    {
        if (data is float healAmount)
        {
            _currentHp = Mathf.Min(_maxHp, _currentHp + healAmount);
             EventBus.Emit(GameEvent.PlayerHealthUpdated, new object[] { _currentHp, _maxHp });
        }
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