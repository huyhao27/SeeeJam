using System.Collections;
using UnityEngine;

public class NoteScanner : MonoBehaviour
{
    [SerializeField] private Transform startPoint; // điểm bắt đầu
    [SerializeField] private Transform endPoint;   // điểm kết thúc
    [SerializeField] private float moveSpeed = 200f; // tốc độ pixel/giây
    private bool moving = true;

    private void Start()
    {
        // đặt scanner về vị trí start khi bắt đầu
        transform.position = startPoint.position;
    }

    private void Update()
    {
        if (!moving) return;

        // di chuyển theo trục Y
        float newY = Mathf.MoveTowards(
            transform.position.y,
            endPoint.position.y,
            moveSpeed * Time.deltaTime
        );

        transform.position = new Vector3(
            transform.position.x,
            newY,
            transform.position.z
        );

        // khi tới endPoint thì reset lại startPoint
        if (Mathf.Abs(transform.position.y - endPoint.position.y) < 0.01f)
        {
            transform.position = startPoint.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Note note = collision.GetComponent<Note>();
        if (!note) return;

        note.onHit();
    }
}