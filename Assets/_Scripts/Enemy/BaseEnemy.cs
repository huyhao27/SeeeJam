using System.Collections.Generic;
using UnityEngine;
public class BaseEnemy : MonoBehaviour
{
    #region Serialized Fields
    [Header("Stats")]
    [SerializeField] private int maxHp = 30;              
    [SerializeField] private float baseMoveSpeed = 2.5f;
    [Header("Pooling (tuỳ chọn)")]
    [Tooltip("Nếu được gán, enemy sẽ trả về pool của prefab này khi chết")]

    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 6f;   // Phạm vi phát hiện Player để chuyển sang Pursuit
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

    private bool isExternallyStunned = false;      // true -> khoá di chuyể

    // Quản lý vòng đời với object pool
    private bool isPooledManaged = false; // true nếu instance này được spawn từ ObjectPool và sẽ trả về pool khi chết
    
    // Patrol (đi tuần) – di chuyển ngẫu nhiên trong 2D thay vì quay vòng tại chỗ
    [Header("Patrol Settings")]
    [SerializeField] private float patrolSpeedMultiplier = 0.8f; // tốc độ khi patrol
    [SerializeField] private Vector2 patrolChangeDirInterval = new Vector2(1.5f, 3.5f); // thời gian đổi hướng
    private Vector2 patrolDir = Vector2.zero;
    private float patrolTimer = 0f;
    #endregion

    #region Unity Events
    private void Awake()
    {
        currentHp = maxHp;
        currentMoveSpeed = baseMoveSpeed;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        UpdateAI(dt);
    }
    #endregion

    #region Public API
    // Được EnemySpawnManager gọi khi spawn từ pool
    public virtual void OnSpawn()
    {
        // Reset trạng thái để tái sử dụng an toàn
        currentHp = maxHp;
        currentState = State.Idle;
        currentMoveSpeed = baseMoveSpeed;
        currentTarget = null;
        attackTimer = 0f;
        isExternallyStunned = false;
    }

    // Được EnemySpawnManager gọi trước khi trả về pool
    public virtual void OnDespawn()
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
            OnDeath();
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

    // Patrol: di chuyển nhàn rỗi (đi tuần) – di chuyển ngẫu nhiên trong 2D
    public void Patrol()
    {
        patrolTimer -= Time.deltaTime;
        if (patrolTimer <= 0f || patrolDir == Vector2.zero)
        {
            float t = Random.Range(patrolChangeDirInterval.x, patrolChangeDirInterval.y);
            patrolTimer = t;
            patrolDir = Random.insideUnitCircle.normalized;
        }

        float speed = baseMoveSpeed * Mathf.Max(0.1f, patrolSpeedMultiplier);
        Vector3 delta = (Vector3)(patrolDir * speed * Time.deltaTime);
        transform.position += delta;
        if (patrolDir != Vector2.zero)
        {
            transform.up = new Vector3(patrolDir.x, patrolDir.y, 0f);
        }
    }

    // Pursue: đuổi theo Player mục tiêu
    public void Pursue(Transform target)
    {
        if (target == null) return;
        currentTarget = target;
        Vector2 vSelf = transform.position;
        Vector2 vTarget = target.position;
        Vector2 dir = (vTarget - vSelf);
        float dist = dir.magnitude;
        if (dist > 0.05f)
        {
            dir.Normalize();
            transform.position += (Vector3)(dir * currentMoveSpeed * Time.deltaTime);
            transform.up = new Vector3(dir.x, dir.y, 0f); // quay mặt theo 2D
        }
    }

    // DoAttack: tấn công thường (cận chiến) – stub
    public void DoAttack(Transform target)
    {
        if (target == null) return;
        // TODO: Gọi hàm nhận damage của Player (ví dụ target.TakeDamage(contactDamage))
        // Hiện tạm chỉ log.
        Debug.Log($"Enemy {name} Attack -> +{contactDamage} dmg lên Player (placeholder)");
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
            float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
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
                float dist = Vector3.Distance(transform.position, p.position);
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
        SetHp(currentHp - dmg);
        // TODO: Hiệu ứng hit (flash, sound,...)
    }

    private void OnDeath()
    {
        // TODO: Drop loot / spawn effect / thông báo hệ thống progression
        Debug.Log($"Enemy {name} đã chết");
        // Gọi rớt thưởng (hàm rỗng – sẽ triển khai sau)
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    #endregion
}