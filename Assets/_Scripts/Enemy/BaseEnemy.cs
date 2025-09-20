using UnityEngine;

// Sử dụng PlayerDamageEvent struct

public class BaseEnemy : MonoBehaviour, IPoolable
{
    #region Serialized Fields
    [Header("Stats")]
    [SerializeField] private int maxHp = 30;
    [SerializeField] private float baseMoveSpeed = 2.5f;
    [SerializeField] private float knockbackResistance = 0f; // 0 = ăn full, 0.5 = giảm 50%, 1 = miễn nhiễm

    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float attackRange = 1.4f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private int contactDamage = 5;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolSpeedMultiplier = 0.8f;
    [SerializeField] private Vector2 patrolChangeDirInterval = new Vector2(1.5f, 3.5f);

    [Header("Rewards")]
    [Tooltip("Số XP rơi ra (tổng). Nếu dùng randomRange thì bỏ qua giá trị này.")]
    [SerializeField] private int xpReward = 5;
    [Tooltip("Nếu > 0 sẽ random số XP giữa min-max (inclusive). Nếu cả hai = 0 sẽ dùng xpReward.")]
    private Vector2Int xpRewardRandomRange = Vector2Int.zero; // x=min, y=max
    [Tooltip("Level XP prefab trong danh sách XpManager (1 = phần tử đầu). Bạn có thể mapping enemy -> level khác nhau.")]
    [SerializeField] private int xpPrefabLevel = 1;
    [Tooltip("Chia nhỏ XP thành nhiều viên? 1 = không chia. Ví dụ 3 -> spawn 3 orbs.")]
    [SerializeField, Min(1)] private int xpSpawnChunks = 1;
    [Tooltip("Độ lệch vị trí spawn từng orb.")]
    [SerializeField] private float xpScatterRadius = 0.4f;
    #endregion

    #region Runtime State
    // Stats
    private int currentHp;
    private float currentMoveSpeed; // Tốc độ sau khi áp dụng hiệu ứng Slow / Stun

    // AI
    private State currentState = State.Idle;
    protected Transform currentTarget; // Cho phép lớp con (ví dụ RangedEnemy) truy cập target
    protected float attackTimer;       // Cho phép override / tùy biến cooldown

    // Patrol
    private Vector2 patrolDir = Vector2.zero;
    private float patrolTimer = 0f;
    private Vector2 patrolVelocity = Vector2.zero;

    // Pursue
    private Vector2 pursueVelocity = Vector2.zero;

    // Components
    private Rigidbody2D rb;
    private Vector2 externalVelocity = Vector2.zero; // lưu knockback tạm thời
    private float externalVelDamping = 8f; // tốc độ giảm knockback

    [Header("Visual Flip Settings")] 
    [SerializeField] private bool useHorizontalFlip = true; // Nếu tắt sẽ giữ nguyên logic quay cũ
    [SerializeField] private Transform visualRoot;           // Gốc để flip (sprite container). Nếu null dùng transform hiện tại
    private bool facingRight = true;                         // Trạng thái hướng hiện tại

    [Header("Separation (Anti-Overlap)")]
    [Tooltip("Bật để enemy tách nhẹ khỏi nhau khi đứng gần (steering tách chứ không dùng collision).")]
    [SerializeField] private bool enableSeparation = true;
    [Tooltip("Bán kính quét tìm enemy khác để tách.")]
    [SerializeField] private float separationRadius = 0.6f;
    [Tooltip("Lực tách cơ bản.")]
    [SerializeField] private float separationForce = 2.5f;
    [Tooltip("Cho phép vận tốc sau khi cộng separation vượt targetSpeed tối đa bao nhiêu lần.")]
    [SerializeField] private float separationMaxSpeedMultiplier = 1.15f;
    [Tooltip("LayerMask dùng để tìm các enemy khác (chỉ nên tick layer Enemy).")]
    [SerializeField] private LayerMask separationEnemyLayers;

    // Reusable buffer tránh GC
    private static readonly Collider2D[] _separationBuffer = new Collider2D[24];
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

    private void FixedUpdate()
    {
        // Giảm dần externalVelocity (knockback) để blend mượt
        if (externalVelocity.sqrMagnitude > 0.0001f)
        {
            externalVelocity = Vector2.MoveTowards(externalVelocity, Vector2.zero, externalVelDamping * Time.fixedDeltaTime);
            rb.velocity += externalVelocity; // cộng thêm vào vận tốc đang được AI set ở UpdateAI
        }
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

    // Overload nhận thêm hướng & lực knockback (forceMagnitude có thể âm -> bỏ qua)
    public void TakeDamage(int dmg, Vector2 hitDirection, float forceMagnitude)
    {
        TakeDamage(dmg);
        if (dmg > 0 && forceMagnitude > 0f && currentHp > 0)
        {
            ApplyKnockback(hitDirection, forceMagnitude);
        }
    }

    protected virtual void Die()
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
        // XP DROP
        int totalXp = xpReward;
        if (xpRewardRandomRange.x > 0 || xpRewardRandomRange.y > 0)
        {
            int min = Mathf.Min(xpRewardRandomRange.x, xpRewardRandomRange.y);
            int max = Mathf.Max(xpRewardRandomRange.x, xpRewardRandomRange.y);
            totalXp = Random.Range(min, max + 1);
        }
        if (totalXp <= 0 || XpManager.Instance == null) return;

        int chunks = Mathf.Clamp(xpSpawnChunks, 1, 25);
        // Chia đều, phần dư cộng dần vào các orb đầu
        int basePer = totalXp / chunks;
        int remainder = totalXp - basePer * chunks;

        for (int i = 0; i < chunks; i++)
        {
            int amountThis = basePer + (i < remainder ? 1 : 0);
            if (amountThis <= 0) continue;

            Vector2 offset = xpScatterRadius > 0f ? Random.insideUnitCircle * xpScatterRadius : Vector2.zero;
            Vector3 spawnPos = transform.position + new Vector3(offset.x, offset.y, 0f);
            XpManager.Instance.SpawnXpAt(xpPrefabLevel, spawnPos, amountThis);
        }
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
        
        Vector2 finalVel = patrolVelocity;
        if (enableSeparation)
        {
            Vector2 sep = ComputeSeparationForce();
            finalVel += sep;
            float maxAllowed = (baseMoveSpeed * patrolSpeedMultiplier) * separationMaxSpeedMultiplier;
            if (finalVel.magnitude > maxAllowed)
                finalVel = finalVel.normalized * maxAllowed;
        }
        rb.velocity = finalVel;

        if (!useHorizontalFlip)
        {
            if (finalVel.sqrMagnitude > 0.0001f)
            {
                Vector2 lookDir = finalVel.normalized;
                Quaternion targetRot = Quaternion.LookRotation(Vector3.forward, new Vector3(lookDir.x, lookDir.y, 0f));
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, patrolRotateLerp * dt);
            }
        }
        else
        {
            HandleFlip(finalVel.x);
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
        
        Vector2 finalVel = pursueVelocity;
        if (enableSeparation)
        {
            Vector2 sep = ComputeSeparationForce();
            finalVel += sep;
            float maxAllowed = currentMoveSpeed * separationMaxSpeedMultiplier;
            if (finalVel.magnitude > maxAllowed)
                finalVel = finalVel.normalized * maxAllowed;
        }
        rb.velocity = finalVel;

        if (!useHorizontalFlip)
        {
            Vector2 faceDir = finalVel.sqrMagnitude > 0.0001f ? finalVel.normalized : toTarget.normalized;
            if (faceDir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(Vector3.forward, new Vector3(faceDir.x, faceDir.y, 0f));
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, pursueRotateLerp * dt);
            }
        }
        else
        {
            float xDir = finalVel.sqrMagnitude > 0.0001f ? finalVel.x : toTarget.x;
            HandleFlip(xDir);
        }
    }

    protected virtual void DoAttack(Transform target)
    {
        if (target == null) return;
        rb.velocity = Vector2.zero; // Dừng lại khi tấn công

    // Payload đơn giản: [0]=damage(int), [1]=attacker(GameObject), [2]=target(GameObject)
    EventBus.Emit(GameEvent.PlayerDamaged, new object[] { contactDamage, this.gameObject, target.gameObject });
        attackTimer = attackCooldown;
    }
    #endregion
    
    #region Public API
    public virtual void SetHp(int amount)
    {
        currentHp = Mathf.Clamp(amount, 0, maxHp);
        if (currentHp <= 0)
        {
            Die();
        }
    }

    public void ApplyKnockback(Vector2 dir, float force)
    {
        if (force <= 0f) return;
        float resist = Mathf.Clamp01(knockbackResistance);
        float final = force * (1f - resist);
        if (final <= 0f) return;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir = dir.normalized;
        // Thay vì set velocity trực tiếp (gây dật), cộng vào externalVel để được giảm dần
        externalVelocity += dir * final;
    }
    #endregion

    #region Helpers / Debug
    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;
    public State CurrentState => currentState;
    public Transform CurrentTarget => currentTarget;

    #endregion

    #region Flip Logic
    private void HandleFlip(float xMove)
    {
        if (!useHorizontalFlip) return;
        if (Mathf.Abs(xMove) < 0.0001f) return; // Không thay đổi nếu gần như đứng yên ngang

        bool wantRight = xMove > 0f;
        if (wantRight != facingRight)
        {
            facingRight = wantRight;
            var root = visualRoot != null ? visualRoot : transform;
            Vector3 ls = root.localScale;
            ls.x = Mathf.Abs(ls.x) * (facingRight ? 1f : -1f);
            root.localScale = ls;
        }
    }

    private Vector2 ComputeSeparationForce()
    {
        if (!enableSeparation || separationRadius <= 0f || separationForce <= 0f) return Vector2.zero;

        int count = Physics2D.OverlapCircleNonAlloc(rb.position, separationRadius, _separationBuffer, separationEnemyLayers);
        if (count <= 1) return Vector2.zero; // Chỉ có mình hoặc không tìm thấy

        Vector2 acc = Vector2.zero;
        int contributors = 0;
        for (int i = 0; i < count; i++)
        {
            var col = _separationBuffer[i];
            if (col == null) continue;
            if (col.attachedRigidbody == rb) continue; // Bỏ qua chính mình

            Vector2 toMe = (Vector2)rb.position - (Vector2)col.transform.position;
            float dist = toMe.magnitude;
            if (dist < 0.0001f) continue;
            // Trọng số nghịch với khoảng cách bình phương để đẩy mạnh khi rất gần
            acc += toMe / (dist * dist);
            contributors++;
        }

        if (contributors == 0) return Vector2.zero;
        acc /= contributors;
        if (acc == Vector2.zero) return Vector2.zero;
        acc = acc.normalized * separationForce;
        return acc;
    }

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