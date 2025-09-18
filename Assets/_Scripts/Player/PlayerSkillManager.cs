using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    [SerializeField] private List<BaseSkill> skills;

    private PlayerInput playerInput;
    private Camera mainCamera;
    private readonly Dictionary<BaseSkill, float> skillCooldowns = new Dictionary<BaseSkill, float>();

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        mainCamera = Camera.main;
        foreach (var skill in skills)
        {
            if (skill != null) skillCooldowns[skill] = 0f;
        }
    }

    private void OnEnable()
    {
        EventBus.On(GameEvent.ActivateSkill, OnSkillActivateEvent);

        // Input test skill
        // playerInput.OnSkill1Performed += () => TryActivateSkill(0);
        // playerInput.OnSkill2Performed += () => TryActivateSkill(1);
        // playerInput.OnSkill3Performed += () => TryActivateSkill(2);
    }

    private void OnDisable()
    {
        EventBus.Off(GameEvent.ActivateSkill, OnSkillActivateEvent);

        // playerInput.OnSkill1Performed -= () => TryActivateSkill(0);
        // playerInput.OnSkill2Performed -= () => TryActivateSkill(1);
        // playerInput.OnSkill3Performed -= () => TryActivateSkill(2);
    }
    
    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        HandleRotation();
        // UpdateAllCooldowns();
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
            skill.Activate(gameObject);
            skillCooldowns[skill] = skill.Cooldown;
        }
    }

    // private void UpdateAllCooldowns()
    // {
    //     var skillKeys = new List<BaseSkill>(skillCooldowns.Keys);
    //     foreach (var skill in skillKeys)
    //     {
    //         if (skillCooldowns[skill] > 0)
    //         {
    //             skillCooldowns[skill] -= Time.deltaTime;
    //         }
    //     }
    // }

    [SerializeField] private float rotationSpeed = 5f;

    private void HandleRotation()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        float smoothAngle = Mathf.LerpAngle(
            transform.eulerAngles.z,      
            targetAngle,                 
            rotationSpeed * Time.deltaTime 
        );

        transform.rotation = Quaternion.Euler(0f, 0f, smoothAngle);
    }

}