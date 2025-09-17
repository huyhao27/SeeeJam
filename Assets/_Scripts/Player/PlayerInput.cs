using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public event Action<Vector2> OnMove;
    public event Action OnDashPerformed;
    
    public event Action OnSkill1Performed;
    public event Action OnSkill2Performed;
    public event Action OnSkill3Performed;

    private PlayerControls playerControls;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();

        // Move
        playerControls.Player.Move.performed += ctx => OnMove?.Invoke(ctx.ReadValue<Vector2>());
        playerControls.Player.Move.canceled += ctx => OnMove?.Invoke(Vector2.zero);

        // Dash
        playerControls.Player.Dash.performed += _ => OnDashPerformed?.Invoke();

        // Debug skill
        playerControls.Player.Skill1.performed += _ => OnSkill1Performed?.Invoke();
        playerControls.Player.Skill2.performed += _ => OnSkill2Performed?.Invoke();
        playerControls.Player.Skill3.performed += _ => OnSkill3Performed?.Invoke();

    }

    private void OnDisable()
    {
        playerControls.Disable();
        playerControls.Player.Skill1.performed -= _ => OnSkill1Performed?.Invoke();
        playerControls.Player.Skill2.performed -= _ => OnSkill2Performed?.Invoke();
        playerControls.Player.Skill3.performed -= _ => OnSkill3Performed?.Invoke();
    }
}