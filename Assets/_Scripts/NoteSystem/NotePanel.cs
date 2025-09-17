using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
public class NotePanel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform notePrefab;
    [SerializeField] private RectTransform parentPanel;

    [Header("Slot config")]
    [SerializeField] private int columns = 4;     // số cột
    [SerializeField] private int rows = 10;       // số hàng
    [SerializeField] private float xOffset = 100; // khoảng cách ngang
    [SerializeField] private float yOffset = 100; // khoảng cách dọc
    [SerializeField] private Vector2 startPos = new(-150, 200); // vị trí bắt đầu (góc trên trái chẳng hạn)

    private Dictionary<Vector2, bool> slotMap = new();

    void Start()
    {
        GenerateSlots();
    }

    void GenerateSlots()
    {
        slotMap.Clear();

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector2 pos = startPos + new Vector2(col * xOffset, -row * yOffset);
                slotMap[pos] = false;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Convert click sang local trong panel
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentPanel,
            eventData.position,
            eventData.pressEventCamera,
            out localPos
        );

        // Tìm slot gần nhất
        Vector2 nearest = Vector2.zero;
        float minDist = float.MaxValue;
        foreach (var slot in slotMap.Keys)
        {
            float dist = Vector2.Distance(localPos, slot);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = slot;
            }
        }

        // Spawn note ở slot đó
        if (!slotMap[nearest])
        {
            slotMap[nearest] = true;
            var note = Instantiate(notePrefab, parentPanel);
            note.anchoredPosition = nearest;
        }
    }
}
