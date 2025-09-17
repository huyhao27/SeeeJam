using UnityEngine;

// Enum trạng thái cho Enemy
// Tách riêng để dễ tái sử dụng cho tất cả enemy
public enum State
{
    Idle,     
    Patrol,   
    Pursuit, 
    Attack,
}
