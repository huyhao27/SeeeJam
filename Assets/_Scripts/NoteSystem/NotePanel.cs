using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class NotePanel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private List<RectTransform> notePrefabs;
    
    [SerializeField] private RectTransform parentPanel;

    [Header("Slot config")]
    [SerializeField] private int columns = 3;
    [SerializeField] private int rows = 7;
    [SerializeField] private float xOffset = 113;
    [SerializeField] private float yOffset = 200;
    [SerializeField] private Vector2 startPos = new(-150, 200);

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
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentPanel,
            eventData.position,
            eventData.pressEventCamera,
            out localPos
        );

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


        if (slotMap.ContainsKey(nearest) && !slotMap[nearest])
        {
            int columnIndex = Mathf.RoundToInt((nearest.x - startPos.x) / xOffset);

            if (columnIndex >= 0 && columnIndex < notePrefabs.Count && notePrefabs[columnIndex] != null)
            {
                slotMap[nearest] = true;
                
                RectTransform prefabToSpawn = notePrefabs[columnIndex];
                
                var note = Instantiate(prefabToSpawn, parentPanel);
                note.anchoredPosition = nearest;
                
            }
        }
    }
}