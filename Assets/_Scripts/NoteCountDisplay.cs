using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NoteCountDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        EventBus.On(GameEvent.NoteCountChanged, (noteCount) =>
        {
            text.text = "x" + (int)noteCount;
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
