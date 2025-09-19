using UnityEngine;

public class BaseEnemy : MonoBehaviour, IPoolable
{
    #region Serialized Fields
    [Header("Stats")]
    [SerializeField] private int maxHp = 30;
    [SerializeField] private float baseMoveSpeed = 2.5f;

    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float attackRange = 1.4f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private int contactDamage = 5;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolSpeedMultiplier = 0.8f;
    [SerializeField] private Vector2 patrolChangeDirInterval = new Vector2(1.5f, 3.5f);
    #endregion

    #region Runtime State
    // Stats
    private int currentHp;
    private float currentMoveSpeed; // Tốc độ sau khi áp dụng hiệu ứng Slow / Stun

    // AI
    private State currentState = State.Idle;
    private Transform currentTarget;
    private float attackTimer;

    // Patrol
    private Vector2 patrolDir = Vector2.zero;
    private float patrolTimer = 0f;
    private Vector2 patrolVelocity = Vector2.zero;

    // Pursue
    private Vector2 pursueVelocity = Vector2.zero;

    // Components
    private Rigidbody2D rb;
    #endregion

    #region IPoolable Implementation
    private GameObject _originalPrefab;

    public void OnPoolSpawn()
    {
        currentHp = maxHp;
        currentState = State.Idle;
        currentMoveSpeed = baseMoveSpeed;
        currentTarget = null;
        attackTimer = 0f;
        pursueVelocity = Vector2.zero;
        patrolVelocity = Vector2.zero;
        patrolTimer = 0;
        
        gameObject.SetActive(true);
    }

    public void OnPoolDespawn()
    {
        currentTarget = null;
    }

    public void SetOriginalPrefab(GameObject prefab)
    {
        _originalPrefab = prefab;
    }

    public GameObject GetOriginalPrefab()
    {
        return _originalPrefab;
    }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        UpdateAI(Time.deltaTime);
    }
    #endregion

    #region Core Logic
    public void TakeDamage(int dmg)
    {
        if (dmg <= 0 || currentHp <= 0) return;

        currentHp -= dmg;
        // TODO: Thêm hiệu ứng trúng đòn (nháy đỏ, âm thanh,...)

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Bắn sự kiện cho LevelManager và các hệ thống khác biết
        EventBus.Emit(GameEvent.EnemyDied, this);

        // Rớt đồ/kinh nghiệm
        DropReward();

        // Yêu cầu PoolManager "thu hồi" đối tượng này
        PoolManager.Instance.Despawn(this);
    }

    public virtual void DropReward()
    {
        // Logic rớt vật phẩm ở đây
    }
    #endregion

    #region AI Logic
    private void UpdateAI(float dt)
    {
        attackTimer = Mathf.Max(0, attackTimer - dt);

        if (currentTarget)
        {
            float dist = Vector2.Distance(transform.position, currentTarget.position);
            if (dist <= attackRange)
            {
                SetState(State.Attack);
                if (attackTimer <= 0f) DoAttack(currentTarget);
            }
            else if (dist <= detectionRange)
            {
                SetState(State.Pursuit);
                Pursue(currentTarget);
            }
            else
            {
                currentTarget = null;
                SetState(State.Patrol);
            }
        }
        else
        {
            FindTarget();
            if (currentState != State.Pursuit)
            {
                SetState(State.Patrol);
                Patrol();
            }
        }
    }

    private void FindTarget()
    {
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj)
        {
            if (Vector2.Distance(transform.position, playerObj.transform.position) <= detectionRange)
            {
                currentTarget = playerObj.transform;
            }
        }
    }
    
    public void SetState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;
    }

    private void UpdatePatrolDirection()
    {
        patrolTimer = Random.Range(patrolChangeDirInterval.x, patrolChangeDirInterval.y);
        patrolDir = Random.insideUnitCircle.normalized;
    }

    public void Patrol()
    {
        float patrolAcceleration = 20f;
        float patrolRotateLerp = 5f;
        float dt = Time.deltaTime;

        patrolTimer -= dt;
        if (patrolTimer <= 0f || patrolDir == Vector2.zero)
        {
            UpdatePatrolDirection();
        }

        float targetSpeed = baseMoveSpeed * patrolSpeedMultiplier;
        Vector2 desiredVel = patrolDir * targetSpeed;
        patrolVelocity = Vector2.MoveTowards(patrolVelocity, desiredVel, patrolAcceleration * dt);
        
        rb.velocity = patrolVelocity;

        if (patrolVelocity.sqrMagnitude > 0.0001f)
        {
            Vector2 lookDir = patrolVelocity.normalized;
            Quaternion targetRot = Quaternion.LookRotation(Vector3.forward, new Vector3(lookDir.x, lookDir.y, 0f));
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, patrolRotateLerp * dt);
        }
    }

    public void Pursue(Transform target)
    {
        float pursueAcceleration = 25f;
        float pursueRotateLerp = 12f;
        float approachSlowDown = 1.2f;
        float stopDistanceBuffer = 0.12f;
        float dt = Time.deltaTime;

        Vector2 toTarget = (Vector2)target.position - rb.position;
        float dist = toTarget.magnitude;

        float stopDist = Mathf.Max(0f, attackRange - stopDistanceBuffer);
        float slowStart = Mathf.Max(attackRange, attackRange + approachSlowDown);
        float desiredSpeed = currentMoveSpeed;

        if (dist <= slowStart)
        {
            float t = Mathf.InverseLerp(stopDist, slowStart, dist);
            desiredSpeed *= Mathf.Clamp01(t);
        }
        if (dist <= stopDist)
        {
            desiredSpeed = 0f;
        }

        Vector2 desiredVel = dist > 0.0001f ? toTarget.normalized * desiredSpeed : Vector2.zero;
        pursueVelocity = Vector2.MoveTowards(pursueVelocity, desiredVel, pursueAcceleration * dt);
        
        rb.velocity = pursueVelocity;

        Vector2 faceDir = pursueVelocity.sqrMagnitude > 0.0001f ? pursueVelocity.normalized : toTarget.normalized;
        if (faceDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(Vector3.forward, new Vector3(faceDir.x, faceDir.y, 0f));
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, pursueRotateLerp * dt);
        }
    }

    public void DoAttack(Transform target)
    {
        if (target == null) return;
        rb.velocity = Vector2.zero; // Dừng lại khi tấn công
        
        var hp = target.GetComponentInParent<HpSystem>();
        if (hp != null)
        {
            hp.TakeDamage(contactDamage);
        }
        else
        {
            Debug.LogWarning($"[{nameof(BaseEnemy)}] {name} DoAttack: Không tìm thấy HpSystem trên target {target.name}");
        }
        attackTimer = attackCooldown;
    }
    #endregion
    
    #region Public API
    public void SetHp(int amount)
    {
        currentHp = Mathf.Clamp(amount, 0, maxHp);
        if (currentHp <= 0)
        {
            Die();
        }
    }
    #endregion

    #region Helpers / Debug
    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;
    public State CurrentState => currentState;

    private void OnDrawGizmosSelected()
    {
        #if UNITY_EDITOR
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, detectionRange);
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, attackRange);
        #endif
    }
    #endregion
}