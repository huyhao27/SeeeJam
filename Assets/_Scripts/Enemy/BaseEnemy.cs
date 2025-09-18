using System.Collections.Generic;
using UnityEngine;
public class BaseEnemy : MonoBehaviour
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
    #endregion

    #region Runtime State
    private int currentHp;
    private State currentState = State.Idle;
    private float currentMoveSpeed; // speed sau khi áp dụng Slow / Stun
    private Transform currentTarget;

    // Quản lý cooldown tấn công
    private float attackTimer;

    private bool isExternallyStunned = false;      // true -> khoá di chuyển

    // Quản lý vòng đời với object pool
    private bool isPooledManaged = false; 

    
    [Header("Patrol Settings")]
    [SerializeField]private float patrolSpeedMultiplier = 0.8f;
    private Vector2 patrolChangeDirInterval = new Vector2(1.5f, 3.5f);
    private Vector2 patrolDir = Vector2.zero;
    private float patrolTimer = 0f;
    private Vector2 patrolVelocity = Vector2.zero; 
    private Rigidbody2D rb;

    // Hằng số/tham số phụ
    private const float MinSpeedMultiplier = 0.15f; 
    private float patrolAcceleration = 20f; 
    private float patrolRotateLerp = 5f;     

    // Pursue smoothing
    private Vector2 pursueVelocity = Vector2.zero;            
    private float pursueAcceleration = 25f;  
    private float pursueRotateLerp = 12f;    
    private float approachSlowDown = 1.2f;   
    private float stopDistanceBuffer = 0.12f;
    #endregion

    #region Unity Events
    private void Awake()
    {
        currentHp = maxHp;
        currentMoveSpeed = baseMoveSpeed;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        UpdateAI(dt);
    }
    #endregion

    #region Public API
    // Được EnemySpawnManager gọi khi spawn từ pool
    public virtual void OnPersistentSpawn()
    {
        // Reset trạng thái để tái sử dụng an toàn
        currentHp = maxHp;
        currentState = State.Idle;
        currentMoveSpeed = baseMoveSpeed;
        currentTarget = null;
        attackTimer = 0f;
        isExternallyStunned = false;
        pursueVelocity = Vector2.zero;
    }

    // Được EnemySpawnManager gọi trước khi trả về pool
    public virtual void OnPersistentDespawn()
    {
        // Nếu cần: tắt VFX, reset animation, huỷ DOT tạm thời, vv.
        currentTarget = null;
    }

    // Gán cờ cho biết instance này do pool quản lý
    public void SetPooledManaged(bool value)
    {
        isPooledManaged = value;
    }

    // SetHp: thiết lập trực tiếp HP hiện tại (giới hạn trong [0, maxHp])
    public void SetHp(int amount)
    {
        currentHp = Mathf.Clamp(amount, 0, maxHp);
        if (currentHp <= 0)
        {
            OnPersistentDeath();
        }
    }

    // Thay đổi state (hiện chỉ dùng: Idle, Patrol, Pursuit, Attack)
    public void SetState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        // Có thể thêm xử lý khi enter state ở đây (ví dụ reset animation, v.v.)
        if (newState == State.Pursuit)
        {
            Debug.Log($"[{name}] Enter Pursuit");
        }
    }

    // Cập nhật hướng patrol ngẫu nhiên
    private void UpdatePatrolDirection()
    {
        patrolTimer = Random.Range(patrolChangeDirInterval.x, patrolChangeDirInterval.y);
        patrolDir = Random.insideUnitCircle.normalized;
    }

    // Patrol: di chuyển nhàn rỗi (đi tuần) – di chuyển ngẫu nhiên trong 2D, có làm mượt
    public void Patrol()
    {
        float dt = Time.deltaTime;
        patrolTimer -= dt;
        if (patrolTimer <= 0f || patrolDir == Vector2.zero)
        {
            UpdatePatrolDirection();
        }

        // Tốc độ mục tiêu và vận tốc được làm mượt theo gia tốc
        float targetSpeed = baseMoveSpeed * Mathf.Max(MinSpeedMultiplier, patrolSpeedMultiplier);
        Vector2 desiredVel = patrolDir * targetSpeed;
        patrolVelocity = Vector2.MoveTowards(patrolVelocity, desiredVel, patrolAcceleration * dt);
        Vector2 movement = patrolVelocity * dt;

        // Di chuyển: ưu tiên Rigidbody2D nếu có (an toàn va chạm). Fallback: transform
        if (rb != null && rb.isKinematic == false)
        {
            rb.MovePosition(rb.position + movement);
        }
        else
        {
            transform.position += (Vector3)movement;
        }

        // Xoay mượt về hướng di chuyển
        if (patrolVelocity.sqrMagnitude > 0.0001f)
        {
            Vector2 lookDir = patrolVelocity.normalized;
            Quaternion targetRot = Quaternion.LookRotation(Vector3.forward, new Vector3(lookDir.x, lookDir.y, 0f));
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, patrolRotateLerp * dt);
        }
    }

    // Pursue: đuổi theo Player mục tiêu
    public void Pursue(Transform target)
    {
        if (target == null) return;
        currentTarget = target;
        float dt = Time.deltaTime;
        Vector2 vSelf = transform.position;
        Vector2 vTarget = target.position;
        Vector2 toTarget = (vTarget - vSelf);
        float dist = toTarget.magnitude;

        // Tính tốc độ mong muốn (arrival): giảm tốc khi gần tầm đánh
        float stopDist = Mathf.Max(0f, attackRange - stopDistanceBuffer);
        float slowStart = Mathf.Max(attackRange, attackRange + approachSlowDown);
        float desiredSpeed = currentMoveSpeed;
        if (dist <= slowStart)
        {
            float t = Mathf.InverseLerp(stopDist, slowStart, dist); // 0 tại stopDist, 1 tại slowStart
            desiredSpeed = currentMoveSpeed * Mathf.Clamp01(t);
        }
        if (dist <= stopDist)
        {
            desiredSpeed = 0f;
        }

        Vector2 desiredVel = dist > 0.0001f ? toTarget.normalized * desiredSpeed : Vector2.zero;
        pursueVelocity = Vector2.MoveTowards(pursueVelocity, desiredVel, pursueAcceleration * dt);

        // Di chuyển bằng Rigidbody2D nếu có để tránh xô đẩy do physics depenetration
        Vector2 movement = pursueVelocity * dt;
        if (rb != null && rb.isKinematic == false)
        {
            rb.MovePosition(rb.position + movement);
        }
        else
        {
            transform.position += (Vector3)movement;
        }

        // Xoay mượt theo hướng di chuyển (hoặc nhìn vào target nếu đứng yên)
        Vector2 faceDir = pursueVelocity.sqrMagnitude > 0.0001f ? pursueVelocity.normalized : (toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : transform.up);
        Quaternion targetRot = Quaternion.LookRotation(Vector3.forward, new Vector3(faceDir.x, faceDir.y, 0f));
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, pursueRotateLerp * dt);
    }

    // DoAttack: tấn công thường (cận chiến) – stub
    public void DoAttack(Transform target)
    {
        if (target == null) return;
        // Gây sát thương lên Player thông qua HpSystem (nếu có)
        var hp = target.GetComponentInParent<HpSystem>();
        if (hp != null)
        {
            hp.TakeDamage(contactDamage);
        }
        else
        {
            Debug.LogWarning($"[{nameof(BaseEnemy)}] {name} DoAttack: Không tìm thấy HpSystem trên target {target.name}");
        }
        attackTimer = attackCooldown; // reset CD
    }

    // Overload cho signature theo yêu cầu (Effect e) – vì Effect chưa define, ta tạo stub
    public void ApplyEffect(object effect)
    {
        // TODO: Khi có class Effect: đọc type, magnitude, duration... và gọi ApplyBurn/ApplySlow/ApplyStun tương ứng
        Debug.LogWarning("ApplyEffect(object) chưa được hiện thực chi tiết vì lớp Effect chưa tồn tại trong file này.");
    }
    #endregion
    #region AI Update
    // Cập nhật hành vi AI mỗi frame
    private void UpdateAI(float dt)
    {
        // Nếu đang bị stun bởi hệ thống effect bên ngoài thì không làm gì
        if (isExternallyStunned) return;

        attackTimer -= dt;

        // Nếu có target thì kiểm tra khoảng cách, nếu không thì tìm player (stub)
        if (currentTarget)
        {
            float dist = Vector2.Distance(transform.position, currentTarget.transform.position);
            if (dist <= attackRange)
            {
                if (attackTimer <= 0f)
                {
                    SetState(State.Attack);
                    DoAttack(currentTarget);
                }
            }
            else if (dist <= detectionRange)
            {
                SetState(State.Pursuit);
                Pursue(currentTarget);
            }
            else
            {
                // Ra khỏi phạm vi -> quay về Idle/Patrol
                currentTarget = null;
                SetState(State.Patrol);
                Patrol();
            }
        }
        else
        {
            // Tìm Player gần nhất (stub: tìm theo tag "Player")
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj)
            {
                Transform p = playerObj.transform;
                float dist = Vector2.Distance(transform.position, p.position);
                if (dist <= detectionRange)
                {
                    currentTarget = p;
                    SetState(State.Pursuit);
                }
            }

            if (currentState == State.Idle || currentState == State.Patrol)
            {
                SetState(State.Patrol);
                Patrol();
            }
        }
    }
    #endregion

    #region Damage / Death
    // Gây damage lên enemy
    public void TakeDamage(int dmg)
    {
        if (dmg <= 0 || currentHp <= 0) return;
        Debug.Log($"Enemy {name} nhận {dmg} dmg");
        SetHp(currentHp - dmg);
        // TODO: Hiệu ứng hit (flash, sound,...)
    }

    private void OnPersistentDeath()
    {
        // TODO: Drop loot / spawn effect / thông báo hệ thống progression
        Debug.Log($"Enemy {name} đã chết");
        DropReward();
        // Trả enemy về pool Utils nếu có prefab gốc, nếu không thì huỷ đối tượng
        if (isPooledManaged)
        {
            ObjectPool<BaseEnemy>.Instance.Return(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    // Hàm rỗng rớt thưởng – sẽ được cài đặt ở lớp con hoặc khi có hệ thống loot
    public virtual void DropReward()
    {
        // TODO: spawn item/buff/skill theo tier, nối vào progression
    }

    #region Helpers / Debug
    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;
    public State CurrentState => currentState;
    public bool IsPooledManaged => isPooledManaged;

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        // Vẽ vòng tròn 2D trên mặt phẳng XY bằng Handles
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, detectionRange);
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, attackRange);
#else
        // Fallback (nếu không có UnityEditor), vẫn vẽ được trong môi trường runtime
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
#endif
    }
    #endregion
}