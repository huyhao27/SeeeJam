using System.Collections;
using UnityEngine;

public class BossEnemy : BaseEnemy
{
    [Header("Boss Base Settings")] 
    [SerializeField] private string bossName = "Boss";
    [SerializeField] private float skillIntervalBase = 5f;
    [SerializeField] private float skillIntervalPhase2 = 4f; // <= 2/3 HP
    [SerializeField] private float skillIntervalPhase3 = 3f; // <= 1/3 HP
    [SerializeField] private float decisionDelayAfterSkill = 0.25f; // nhỏ để tránh spam ngay lập tức

    [Header("Skill1 Spread Volley")] 
    [SerializeField] private EnemyBullet spreadBulletPrefab; // reuse enemy bullet
    [SerializeField] private int volleyCount = 5;
    [SerializeField] private int bulletsPerVolley = 7;
    [SerializeField] private float spreadAngle = 60f;
    [SerializeField] private float delayBetweenVolleys = 0.25f;
    [SerializeField] private float spreadBulletDamageMultiplier = 1f; // nếu cần chỉnh damage (sau này có thể thêm API)

    [Header("Skill2 Fast Orb")] 
    [SerializeField] private BossOrbProjectile orbPrefab;
    [SerializeField] private float orbDamage = 25f;
    [SerializeField] private float orbExplosionRadius = 2.5f;

    [Header("Skill3 Bomb Field")]
    [SerializeField] private BossBomb bombPrefab;
    [SerializeField] private int bombCount = 6;
    [SerializeField] private float bombSpawnRadius = 6f;
    [SerializeField] private float bombDamage = 20f;

    [Header("Skill4 Charge")]
    [SerializeField] private float chargeWindup = 2f;
    [SerializeField] private float chargeSpeed = 18f;
    [SerializeField] private float chargeDuration = 0.5f;
    [SerializeField] private int chargeDamage = 30;
    [SerializeField] private float stunDuration = 3f;

    [Header("Skill5 Summon")]
    [SerializeField] private BaseEnemy meleePrefab;
    [SerializeField] private BaseEnemy rangedPrefab;
    [SerializeField] private int summonMeleeCount = 5;
    [SerializeField] private int summonRangedCount = 5;
    [SerializeField] private float summonRadius = 5f;

    [Header("Runtime / Debug")] 
    [SerializeField] private bool logSkills = true;
    [SerializeField] private bool debugDetection = true;
    [SerializeField] private bool debugIntervals = false;
    [SerializeField] private bool debugOverlay = false;
    [SerializeField] private Color gizmoBombColor = new Color(1f,0.6f,0.1f,0.25f);
    [SerializeField] private Color gizmoSummonColor = new Color(0.3f,0.8f,1f,0.15f);

    private bool isCasting;
    private Coroutine skillLoopCo;
    private Transform player;
    private Rigidbody2D rb2d;

    protected override void Die()
    {
        base.Die();
        EventBus.Emit(GameEvent.BossDied, this);
        if (skillLoopCo != null) StopCoroutine(skillLoopCo);
    }

    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj) {
            player = playerObj.transform;
            if (debugDetection) Debug.Log("[Boss] Found player at start: " + player.position);
        }
        EventBus.Emit(GameEvent.BossSpawned, this);
        skillLoopCo = StartCoroutine(SkillLoop());
        EmitHpChanged();
    }

    public override void SetHp(int amount)
    {
        base.SetHp(amount);
        EmitHpChanged();
    }

    public void ApplyDamage(int dmg)
    {
        TakeDamage(dmg);
        EmitHpChanged();
    }

    private void EmitHpChanged()
    {
        EventBus.Emit(GameEvent.BossHpChanged, new object[]{ CurrentHp, MaxHp });
    }

    private float GetCurrentInterval()
    {
        float hpRatio = (float)CurrentHp / MaxHp;
        if (hpRatio <= 1f/3f) return skillIntervalPhase3;
        if (hpRatio <= 2f/3f) return skillIntervalPhase2;
        return skillIntervalBase;
    }

    private IEnumerator SkillLoop()
    {
        while (CurrentHp > 0)
        {
            float wait = GetCurrentInterval();
            if (debugIntervals) Debug.Log($"[Boss] Interval={wait:F2}s HP%={(float)CurrentHp/MaxHp:0.00}");
            yield return new WaitForSeconds(wait);
            if (isCasting) { yield return null; continue; }
            int skillId = Random.Range(1,6); // 1..5
            CastSkill(skillId);
            yield return new WaitForSeconds(decisionDelayAfterSkill);
        }
    }

    private void CastSkill(int id)
    {
        if (logSkills) Debug.Log($"[Boss] Cast skill {id} -> {GetSkillName(id)}");
        EventBus.Emit(GameEvent.BossSkillCast, id);
        switch(id)
        {
            case 1: StartCoroutine(C_Skill1_Spread()); break;
            case 2: StartCoroutine(C_Skill2_Orb()); break;
            case 3: StartCoroutine(C_Skill3_Bombs()); break;
            case 4: StartCoroutine(C_Skill4_Charge()); break;
            case 5: Skill5_Summon(); break;
        }
    }

    private string GetSkillName(int id)
    {
        switch(id)
        {
            case 1: return "SPREAD_VOLLEY";
            case 2: return "FAST_ORB";
            case 3: return "BOMB_FIELD";
            case 4: return "CHARGE";
            case 5: return "SUMMON";
        }
        return "UNKNOWN";
    }

    #region Skill Coroutines
    private IEnumerator C_Skill1_Spread()
    {
        isCasting = true;
        if (!player || spreadBulletPrefab == null) { isCasting = false; yield break; }
        if (logSkills) Debug.Log("[Boss][Skill1] Start Spread Volley");
        Vector2 dirToPlayer = (player.position - transform.position).normalized;
        for (int v = 0; v < volleyCount; v++)
        {
            FireSpreadVolley(dirToPlayer);
            yield return new WaitForSeconds(delayBetweenVolleys);
        }
        if (logSkills) Debug.Log("[Boss][Skill1] End Spread Volley");
        isCasting = false;
    }

    private void FireSpreadVolley(Vector2 forward)
    {
        if (bulletsPerVolley <= 0 || spreadBulletPrefab == null) return;

        // Lấy hướng chuẩn ban đầu (forward đã chuẩn hóa bên ngoài)
        // Ta muốn tạo một “fan” đều từ -spreadAngle/2 -> +spreadAngle/2
        float totalAngle = spreadAngle;
        float start = -totalAngle * 0.5f;
        // Nếu chỉ 1 viên: bắn thẳng forward
        if (bulletsPerVolley == 1)
        {
            var one = PoolManager.Instance.Spawn(spreadBulletPrefab, transform.position, Quaternion.LookRotation(Vector3.forward, forward));
            if (one != null) one.Launch(forward);
            return;
        }

        float step = totalAngle / (bulletsPerVolley - 1); // chia đều
        for (int i = 0; i < bulletsPerVolley; i++)
        {
            float ang = start + step * i; // góc hiện tại
            Vector2 dir = Quaternion.Euler(0,0, ang) * forward;
            var bullet = PoolManager.Instance.Spawn(spreadBulletPrefab, transform.position, Quaternion.LookRotation(Vector3.forward, dir));
            if (bullet != null)
            {
                bullet.Launch(dir);
            }
        }
    }

    private IEnumerator C_Skill2_Orb()
    {
        isCasting = true;
        if (!player || orbPrefab == null) { isCasting = false; yield break; }
        Vector2 dir = (player.position - transform.position).normalized;
        var orb = PoolManager.Instance.Spawn(orbPrefab, transform.position, Quaternion.identity);
        if (orb != null)
        {
            orb.Setup(orbDamage, orbExplosionRadius);
            orb.Launch(dir);
            if (logSkills) Debug.Log($"[Boss][Skill2] Spawn Orb dmg={orbDamage} radius={orbExplosionRadius} dir={dir}");
        }
        // orb tự xử lý nổ bằng lifetime
        yield return null;
        isCasting = false;
    }

    private IEnumerator C_Skill3_Bombs()
    {
        isCasting = true;
        if (bombPrefab == null) { isCasting = false; yield break; }
        if (logSkills) Debug.Log($"[Boss][Skill3] Spawning {bombCount} bombs radius={bombSpawnRadius}");
        for (int i = 0; i < bombCount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * bombSpawnRadius;
            var bomb = PoolManager.Instance.Spawn(bombPrefab, transform.position + (Vector3)offset, Quaternion.identity);
            if (bomb != null) bomb.Setup(bombDamage);
        }
        yield return null;
        isCasting = false;
    }

    private IEnumerator C_Skill4_Charge()
    {
        isCasting = true;
        if (!player || rb2d == null) { isCasting = false; yield break; }
        EventBus.Emit(GameEvent.BossChargeStart, null);
        if (logSkills) Debug.Log("[Boss] Charge windup start");
        // Windup
        float timer = 0f;
        while (timer < chargeWindup) { timer += Time.deltaTime; yield return null; }

        // Capture direction at start of dash
        Vector2 dir = (player.position - transform.position).normalized;
        if (logSkills) Debug.Log("[Boss] Charge dash direction: " + dir);
        float dashT = 0f;
        while (dashT < chargeDuration)
        {
            dashT += Time.deltaTime;
            rb2d.velocity = dir * chargeSpeed;
            yield return null;
        }
        rb2d.velocity = Vector2.zero;
        if (logSkills) Debug.Log("[Boss] Charge end");
        isCasting = false;
    }

    private void Skill5_Summon()
    {
        if (meleePrefab != null)
        {
            for (int i = 0; i < summonMeleeCount; i++)
            {
                Vector2 off = Random.insideUnitCircle * summonRadius;
                PoolManager.Instance.Spawn(meleePrefab, transform.position + (Vector3)off, Quaternion.identity);
            }
        }
        if (rangedPrefab != null)
        {
            for (int i = 0; i < summonRangedCount; i++)
            {
                Vector2 off = Random.insideUnitCircle * summonRadius;
                PoolManager.Instance.Spawn(rangedPrefab, transform.position + (Vector3)off, Quaternion.identity);
            }
        }
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Charge impact detection (simple): if currently charging (isCasting && velocity magnitude high) and hit player
        if (isCasting && other.CompareTag("Player"))
        {
            // Gây damage + stun event
            EventBus.Emit(GameEvent.PlayerDamaged, (float)chargeDamage);
            EventBus.Emit(GameEvent.PlayerStunned, stunDuration);
            if (logSkills) Debug.Log("[Boss] Charge impact -> stunned player for " + stunDuration + "s");
        }
    }

    protected override void DoAttack(Transform target)
    {
        // Override để vô hiệu hóa contact damage mặc định (boss chỉ gây damage qua skill)
        // Nếu muốn cho boss cắn/quất sát thương gần thì bật lại logic base bằng cách gọi base.DoAttack(target);
        // Giữ cooldown để state máy không spam.
        attackTimer = 0.75f; // cooldown ngắn nhẹ để boss vẫn đổi state
        if (logSkills)
        {
            Debug.Log($"[Boss] Suppressed base contact attack vs {target?.name} (no direct contact damage)");
        }
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        // Dùng Handles.DrawWireDisc để đồng bộ 2D (như BaseEnemy)
        UnityEditor.Handles.color = new Color(1f, 0.9f, 0f, 0.9f);
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, 6f); // detection fallback

        UnityEditor.Handles.color = new Color(1f, 0f, 0f, 0.9f);
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, 1.4f); // attack fallback

        UnityEditor.Handles.color = gizmoBombColor;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, bombSpawnRadius);

        UnityEditor.Handles.color = gizmoSummonColor;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, summonRadius);
#endif
    }

    private void OnGUI()
    {
        if (!debugOverlay) return;
        if (Camera.main == null) return;
        Vector3 screen = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
        if (screen.z < 0) return;
        string txt = $"Boss HP {CurrentHp}/{MaxHp}\nCasting={isCasting}\nInterval={GetCurrentInterval():F2}";
        GUI.Label(new Rect(screen.x - 60, Screen.height - screen.y - 30, 160, 60), txt);
    }
}
