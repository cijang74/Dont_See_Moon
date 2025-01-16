using UnityEngine;
using TMPro;

public class DragToRotate : MonoBehaviour
{
    private Vector2 startMousePosition;
    private Vector2 startObjectDirection;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 마우스 클릭 시작
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (IsMouseOnObject(mousePosition))
            {
                RadioManagerScript.Instance.isDragging = true;
                startMousePosition = mousePosition;
                startObjectDirection = (mousePosition - (Vector2)transform.position).normalized;
            }
        }

        if (Input.GetMouseButton(0) && RadioManagerScript.Instance.isDragging) // 드래그 중
        {
            Vector2 currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 currentDirection = (currentMousePosition - (Vector2)transform.position).normalized;

            float angle = Vector2.SignedAngle(startObjectDirection, currentDirection);
            transform.Rotate(Vector3.forward, angle);

            RadioManagerScript.Instance.radioFrequency -= angle * RadioManagerScript.Instance.ratioChangeSpeed * Time.deltaTime;

            startMousePosition = currentMousePosition;
            startObjectDirection = currentDirection;
        }

        if (Input.GetMouseButtonUp(0)) // 마우스 클릭 해제
        {
            RadioManagerScript.Instance.isDragging = false;
        }

    }

    private bool IsMouseOnObject(Vector2 mousePosition)
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            return collider.OverlapPoint(mousePosition); //마우스 좌표가 오브젝트의 콜라이더 위에 있는지 판별
        }
        return false;
    }

}
