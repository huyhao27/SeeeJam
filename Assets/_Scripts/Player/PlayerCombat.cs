using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class PlayerCombat : MonoBehaviour
{
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        playerInput.OnAttackPerformed += HandleBaseAttack;
    }

    private void OnDisable()
    {
        playerInput.OnAttackPerformed -= HandleBaseAttack;
    }

    /// <summary>
    /// base attack
    /// </summary>
    private void HandleBaseAttack()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        // TODO: Logic attack here
        Debug.Log("Player performed a base attack! (Space pressed)");
    }
}