using UnityEngine;

[CreateAssetMenu(fileName = "New WaveData", menuName = "Game/Wave Data (Fixed Quantity)")]
public class WaveData : ScriptableObject
{
    [Header("Wave Timing")]
    [Tooltip("Wave này bắt đầu tại mốc % thời gian của màn chơi.")]
    [Range(0f, 1f)]
    public float startTimePercentage = 0f;

    [Header("Spawning Cadence")]
    [Tooltip("Thời gian chờ giữa mỗi lượt spawn.")]
    public float spawnInterval = 1f;
    [Tooltip("Số lượng quái spawn trong một lượt.")]
    public int spawnCountPerTick = 2;

    [Header("Enemy Composition")]
    [Tooltip("Tổng số quái sẽ được spawn trong wave này.")]
    public int totalEnemyCount = 50;

    [Header("Enemy Type A")]
    public BaseEnemy enemyTypeA;
    [Tooltip("Tỷ lệ của Enemy A. Enemy B sẽ là phần còn lại.")]
    [Range(0f, 1f)]
    public float ratioTypeA = 0.7f;

    [Header("Enemy Type B")]
    [Tooltip("Loại quái thứ hai. Có thể để trống nếu wave chỉ có 1 loại quái.")]
    public BaseEnemy enemyTypeB;
}