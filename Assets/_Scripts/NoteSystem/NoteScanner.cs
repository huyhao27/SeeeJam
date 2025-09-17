using System.Collections;
using UnityEngine;

public class NoteScanner : MonoBehaviour
{
    [SerializeField] private Transform startPoint; // điểm bắt đầu
    [SerializeField] private Transform endPoint;   // điểm kết thúc
    [SerializeField] private float moveSpeed = 200f; // tốc độ pixel/giây

    private Coroutine moveRoutine;

    private void Start()
    {
        Debug.Log("ngon");
        // đặt scanner về vị trí start khi bắt đầu
        transform.position = startPoint.position;
        moveRoutine = StartCoroutine(MoveLoop());
    }

    private IEnumerator MoveLoop()
    {
        while (true)
        {
            // đi từ start → end
            yield return MoveTo(endPoint.position);

            // reset lại start
            transform.position = startPoint.position;
        }
    }

    private IEnumerator MoveTo(Vector3 targetPos)
    {
        while (Mathf.Abs(transform.position.y - targetPos.y) > 0.01f)
        {
            Vector3 currentPos = transform.position;

            // chỉ move theo Y
            float newY = Mathf.MoveTowards(
                currentPos.y,
                targetPos.y,
                moveSpeed * Time.deltaTime
            );

            transform.position = new Vector3(
                currentPos.x,   // giữ nguyên X
                newY,           // thay đổi Y
                currentPos.z    // giữ nguyên Z
            );

            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {

        Note note = collision.GetComponent<Note>();
        if (!note) return;
        note.onHit();
    }
}
