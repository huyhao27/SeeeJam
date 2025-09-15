using System.Collections.Generic;
using UnityEngine;

#region TEMP_STUB_TYPES
// Các định nghĩa tạm thời để tránh lỗi biên dịch nếu project CHƯA có thực sự.
// Khi đã có enum Element, State và lớp Player riêng, define một scripting symbol (ví dụ: GAME_TYPES_DEFINED)
// trong Player Settings rồi xoá / comment khối này, hoặc đơn giản xoá hẳn.
#if !GAME_TYPES_DEFINED
public enum Element { Fire, Ice, Shock }
public enum State { Idle, Patrol, Pursuit, Attack, Stunned, Slowed, Burning, Frozen }
public class Player : MonoBehaviour { }
#endif
#endregion

public class EnemyEntity : MonoBehaviour
{
    #region Serialized Fields
    [Header("Stats")]
    [SerializeField] private int maxHp = 30;              
    [SerializeField] private float baseMoveSpeed = 2.5f;
    [SerializeField] private Element element = Element.Fire;

    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 6f;   // Phạm vi phát hiện Player để chuyển sang Pursuit
    [SerializeField] private float attackRange = 1.4f;
    [SerializeField] private float attackCooldown = 1.2f; 
    [SerializeField] private int contactDamage = 5;

    [Header("Effect Settings")]
    [SerializeField] private float burnTickInterval = 0.5f;  // Khoảng thời gian giữa các tick Burn
    [SerializeField] private int burnDamagePerTick = 1;
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private float stunLockMoveSpeed = 0f;   // Tốc độ khi bị stun
    [SerializeField] private float defaultStunDuration = 0.8f;
    [Header("Combo Window (Elemental)")]
    [Tooltip("Khoảng thời gian cho phép 2 nguyên tố kế tiếp nhau để tính combo")] 
    [SerializeField] private float comboWindow = 1.2f; // thời gian tối đa giữa 2 lần áp dụng element để kích hoạt combo
    #endregion

    #region Runtime State
    private int currentHp;
    private State currentState = State.Idle;
    private float currentMoveSpeed; // speed sau khi áp dụng Slow / Stun
    private Player currentTarget; 

    // Quản lý cooldown tấn công
    private float attackTimer;

    // Quản lý hiệu ứng đang chạy
    private bool hasBurn; private float burnTimeRemaining; private float burnTickTimer;
    private bool hasSlow; private float slowTimeRemaining; private float originalMoveSpeed;
    private bool hasStun; private float stunTimeRemaining;

    // Dùng để kiểm tra combo nguyên tố đơn giản (lần cuối nhận element)
    private Element lastAppliedElement; private float lastElementAppliedTime = -999f;

    // Bộ nhớ tạm để tránh cấp phát new Vector3 nhiều lần
    private static readonly Vector3 zero = Vector3.zero;
    #endregion

    #region Unity Events
    private void Awake()
    {
        currentHp = maxHp;
        originalMoveSpeed = baseMoveSpeed;
        currentMoveSpeed = baseMoveSpeed;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        UpdateEffects(dt);
        UpdateAI(dt);
    }
    #endregion

    #region Public API
    // SetHp: thiết lập trực tiếp HP hiện tại (giới hạn trong [0, maxHp])
    public void SetHp(int amount)
    {
        currentHp = Mathf.Clamp(amount, 0, maxHp);
        if (currentHp <= 0)
        {
            OnDeath();
        }
    }

    // Thay đổi state (có kiểm soát chuyển đổi đơn giản)
    public void SetState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        // Có thể thêm xử lý khi enter state ở đây (ví dụ reset animation, tốc độ, v.v.)
        switch (currentState)
        {
            case State.Stunned:
                // Khi bị stun tốc độ = 0
                currentMoveSpeed = stunLockMoveSpeed;
                break;
            case State.Slowed:
                currentMoveSpeed = originalMoveSpeed * slowMultiplier;
                break;
            default:
                currentMoveSpeed = originalMoveSpeed;
                break;
        }
    }

    // Patrol: di chuyển nhàn rỗi (stub) – hiện tạm quay vòng tại chỗ / random nhỏ
    public void Patrol()
    {
        // TODO: Thay bằng logic tuần tra waypoint / random roam
        transform.Rotate(Vector3.up, 20f * Time.deltaTime);
    }

    // Pursue: đuổi theo Player mục tiêu
    public void Pursue(Player target)
    {
        if (target == null) return;
        currentTarget = target;
        Vector3 dir = (target.transform.position - transform.position);
        dir.y = 0f;
        float dist = dir.magnitude;
        if (dist > 0.05f)
        {
            dir.Normalize();
            transform.position += dir * currentMoveSpeed * Time.deltaTime;
            transform.forward = dir; // quay mặt về player
        }
    }

    // DoAttack: tấn công thường (cận chiến) – stub
    public void DoAttack(Player target)
    {
        if (target == null) return;
        // TODO: Gọi hàm nhận damage của Player (ví dụ target.TakeDamage(contactDamage, element))
        // Hiện tạm chỉ log.
        Debug.Log($"Enemy {name} Attack -> +{contactDamage} dmg lên Player (placeholder)");
        attackTimer = attackCooldown; // reset CD
    }

    // ApplyEffect: áp dụng hiệu ứng cơ bản (Burn / Slow / Stun) từ nguyên tố
    public void ApplyEffect(Element sourceElement, float magnitude, float duration)
    {
        // magnitude: cường độ (dùng mở rộng sau), hiện tại chỉ đơn giản kích hoạt theo type
        // duration: thời lượng hiệu ứng

        // Ghi nhận để kiểm tra combo nguyên tố
        HandleElementCombo(sourceElement);

        switch (sourceElement)
        {
            case Element.Fire:
                ApplyBurn(duration);
                break;
            case Element.Ice:
                ApplySlow(duration);
                break;
            case Element.Shock:
                ApplyStun(duration <= 0 ? defaultStunDuration : duration);
                break;
        }
    }

    // Overload cho signature theo yêu cầu (Effect e) – vì Effect chưa define, ta tạo stub
    public void ApplyEffect(object effect)
    {
        // TODO: Khi có class Effect: đọc type / element, magnitude, duration...
        Debug.LogWarning("ApplyEffect(object) chưa được hiện thực chi tiết vì lớp Effect chưa tồn tại trong file này.");
    }
    #endregion

    #region Effects Implementation
    private void ApplyBurn(float duration)
    {
        hasBurn = true;
        burnTimeRemaining = Mathf.Max(burnTimeRemaining, duration); // làm mới nếu duration lớn hơn
        // Không đổi state nếu đang stun / frozen... (ưu tiên Stun > Slow > Burn)
        if (!hasStun && !hasSlow)
        {
            SetState(State.Burning);
        }
    }

    private void ApplySlow(float duration)
    {
        hasSlow = true;
        slowTimeRemaining = Mathf.Max(slowTimeRemaining, duration);
        SetState(State.Slowed);
    }

    private void ApplyStun(float duration)
    {
        hasStun = true;
        stunTimeRemaining = Mathf.Max(stunTimeRemaining, duration);
        SetState(State.Stunned);
    }

    private void UpdateEffects(float dt)
    {
        // Burn tick
        if (hasBurn)
        {
            burnTimeRemaining -= dt;
            burnTickTimer -= dt;
            if (burnTickTimer <= 0f)
            {
                burnTickTimer = burnTickInterval;
                TakeDamage(burnDamagePerTick); // damage theo thời gian
            }
            if (burnTimeRemaining <= 0f)
            {
                hasBurn = false;
                burnTickTimer = 0f;
                // Nếu state đang Burning thì reset về Idle (trừ khi có slow/stun)
                if (currentState == State.Burning)
                {
                    SetState(State.Idle);
                }
            }
        }

        // Slow
        if (hasSlow)
        {
            slowTimeRemaining -= dt;
            if (slowTimeRemaining <= 0f)
            {
                hasSlow = false;
                if (currentState == State.Slowed)
                    SetState(State.Idle);
            }
        }

        // Stun
        if (hasStun)
        {
            stunTimeRemaining -= dt;
            if (stunTimeRemaining <= 0f)
            {
                hasStun = false;
                if (currentState == State.Stunned)
                    SetState(State.Idle);
            }
        }
    }
    #endregion

    #region AI Update
    // Cập nhật hành vi AI mỗi frame
    private void UpdateAI(float dt)
    {
        // Nếu đang stun thì không làm gì
        if (hasStun) return;

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
                Player p = playerObj.GetComponent<Player>();
                if (p)
                {
                    float dist = Vector3.Distance(transform.position, p.transform.position);
                    if (dist <= detectionRange)
                    {
                        currentTarget = p;
                        SetState(State.Pursuit);
                    }
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
        Destroy(gameObject);
    }
    #endregion

    #region Element Combo (Stub)
    private void HandleElementCombo(Element newElement)
    {
        if (Time.time - lastElementAppliedTime <= comboWindow)
        {
            // Có khả năng tạo combo (newElement + lastAppliedElement) – xử lý stub
            if (lastAppliedElement == newElement)
            {
                // Ví dụ: Fire + Fire -> nhỏ AoE Burn (placeholder)
                TriggerSameElementCombo(newElement);
            }
            else
            {
                // Ví dụ: Fire + Ice -> Big Explosion (placeholder)
                TriggerDifferentElementCombo(lastAppliedElement, newElement);
            }
        }
        lastAppliedElement = newElement;
        lastElementAppliedTime = Time.time;
    }

    private void TriggerSameElementCombo(Element e)
    {
        // TODO: Thực tế sẽ spawn VFX + áp dụng hiệu ứng AoE.
        Debug.Log($"[Combo] {e}+{e} trên enemy {name} (placeholder)");
    }

    private void TriggerDifferentElementCombo(Element a, Element b)
    {
        // TODO: Thực tế spawn VFX / tính damage / áp dụng CC.
        Debug.Log($"[Combo] {a}+{b} trên enemy {name} (placeholder)");
    }
    #endregion

    #region Helpers / Debug
    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;
    public State CurrentState => currentState;
    public Element ElementType => element; // tránh trùng tên type

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    #endregion
}

/*
 GỢI Ý CÁC PHƯƠNG THỨC / MỞ RỘNG TIẾP THEO
 1. InitStatsFromScriptable(EnemyDefinition def) : load hp/speed/element từ ScriptableObject.
 2. TakeElementalDamage(int baseDamage, Element sourceElement) : tính kháng / cộng dồn.
 3. Knockback(Vector3 force) : đẩy lùi khi trúng đạn / explosion.
 4. Freeze(float duration) : trạng thái đóng băng cứng (phân biệt với Slow).
 5. ClearAllEffects() : xóa sạch hiệu ứng khi vào Safe Room hoặc chuyển phase boss.
 6. OnSpawn() / OnDespawn() : chuẩn hoá life-cycle với object pooling.
 7. SetTarget(Player p) : ép target bên ngoài (manager) thay vì tự tìm.
 8. AttackAnimationEvent() : đồng bộ damage với animation (nếu dùng Animator).
 9. DropReward() : logic rớt skill/buff theo tier.
10. CalculateThreat() : phục vụ hệ thống AI ưu tiên mục tiêu.
11. OnBeatPulse() : callback mỗi nhịp (rhythm) để sync hành vi (di chuyển / attack đúng beat).
12. SerializeRuntimeState() / LoadRuntimeState() : phục vụ save mid-run (nếu cần).
*/
