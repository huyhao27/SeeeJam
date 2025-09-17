using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    private Camera mainCamera;
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        playerInput.OnAttackPerformed += HandleBaseAttack;
    }

    private void OnDisable()
    {
        playerInput.OnAttackPerformed -= HandleBaseAttack;
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        HandleRotation();
    }

    private void HandleRotation()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void HandleBaseAttack()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        Bullet bullet = ObjectPool<Bullet>.Instance.Get();
        if (bullet == null) return;
        
        bullet.transform.position = firePoint.position;

        Vector2 fireDirection = transform.up; 

        bullet.Launch(fireDirection);
        
        Debug.Log("Player fired a bullet!");
    }
}