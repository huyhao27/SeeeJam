using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    private List<BaseSkill> skills => PlayerStats.Instance != null
    ? PlayerStats.Instance.Skills
    : new List<BaseSkill>();

    [Header("Aiming & Rotation")]
    [Tooltip("Gán đối tượng FirePoint vào đây.")]
    [SerializeField] private Transform firePoint;
    [Tooltip("Khoảng cách từ tâm Player đến FirePoint.")]
    [SerializeField] private float firePointRadius = 1.0f;
    [SerializeField] private float rotationSpeed = 10f;

    private PlayerStats playerStats;
    private PlayerInput playerInput;
    private Camera mainCamera;
    private Vector3 originalScale;
    private readonly Dictionary<BaseSkill, float> skillCooldowns = new Dictionary<BaseSkill, float>();


    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerStats = GetComponent<PlayerStats>();

        mainCamera = Camera.main;
        originalScale = transform.localScale;
        foreach (var skill in skills)
        {
            if (skill != null) skillCooldowns[skill] = 0f;
        }
    }

    private void OnEnable()
    {
        EventBus.On(GameEvent.ActivateSkill, OnSkillActivateEvent);
    }

    private void OnDisable()
    {
        EventBus.Off(GameEvent.ActivateSkill, OnSkillActivateEvent);
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        HandleFlipAndAim();
    }
    private void OnSkillActivateEvent(object data)
    {
        if (data is int skillNumber)
        {
            TryActivateSkill(skillNumber);
        }
    }


    public void TryActivateSkill(int skillIndex)
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        if (skillIndex >= 0 && skillIndex < skills.Count && skills[skillIndex] != null)
        {
            BaseSkill skill = skills[skillIndex];
            
            skill.Activate(this, firePoint);
            
            skillCooldowns[skill] = skill.Cooldown;
        }
    }

    private void HandleFlipAndAim()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;

        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
        }
        else if (direction.x > 0)
        {
            transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
        }

        firePoint.position = (Vector2)transform.position + (direction * firePointRadius); // << THAY ĐỔI QUAN TRỌNG

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
        firePoint.rotation = Quaternion.Slerp(firePoint.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}