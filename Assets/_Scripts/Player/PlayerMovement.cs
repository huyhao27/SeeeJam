using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dash")]
    [SerializeField] private float dashDistance = 3f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    private PlayerInput playerInput;
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float lastDashTime;
    private bool isDashing = false;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        playerInput.OnMove += HandleMove;
        playerInput.OnDashPerformed += HandleDash; 
    }

    private void OnDisable()
    {
        playerInput.OnMove -= HandleMove;
        playerInput.OnDashPerformed -= HandleDash; 
    }
    
    private void FixedUpdate()
    {
        if (!isDashing && GameManager.Instance.CurrentState == GameState.Playing)
        {
            rb.velocity = moveDirection * moveSpeed;
        }
    }

    private void HandleMove(Vector2 direction)
    {
        moveDirection = direction;
    }

    private void HandleDash()
    {
        if (Time.time < lastDashTime + dashCooldown || isDashing || GameManager.Instance.CurrentState != GameState.Playing) return;

        isDashing = true;
        lastDashTime = Time.time;
        rb.velocity = Vector2.zero;

        Vector2 targetDirection = moveDirection != Vector2.zero ? moveDirection : (Vector2)transform.right;
        Vector2 targetPosition = rb.position + targetDirection * dashDistance;

        rb.DOMove(targetPosition, dashDuration)
          .SetEase(Ease.OutQuint)
          .OnComplete(() => { isDashing = false; });
    }
}