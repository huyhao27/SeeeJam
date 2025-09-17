
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public event Action<Vector2> OnMove;
    public event Action OnAttackPerformed; // Attack (Space)
    public event Action OnDashPerformed;   // Dash (Shift)

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

        // Attack
        playerControls.Player.Attack.performed += _ => OnAttackPerformed?.Invoke();

        // Dash
        playerControls.Player.Dash.performed += _ => OnDashPerformed?.Invoke();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }
}