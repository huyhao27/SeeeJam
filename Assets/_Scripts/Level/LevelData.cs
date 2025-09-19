using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Timing")]
    [Tooltip("Tổng thời gian cho màn chơi (giây). Sẽ được dùng để tính thời điểm bắt đầu mỗi wave.")]
    public float levelDuration = 900f; // 15 phút

    [Header("Waves")]
    [Tooltip("Danh sách các wave trong màn chơi này. Wave sẽ tuần tự diễn ra.")]
    public List<WaveData> waves;

    [Header("Boss")]
    [Tooltip("Prefab của Boss sẽ xuất hiện ở cuối màn")]
    public BaseEnemy bossPrefab;

    [Tooltip("Vị trí cụ thể để spawn boss. Nếu để trống, sẽ spawn ở (0,0,0)")]
    public Transform bossSpawnPoint;
}