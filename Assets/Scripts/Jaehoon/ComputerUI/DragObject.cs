using UnityEngine;

public class DragObject2D : MonoBehaviour
{
    private bool isDragging = false; // 드래그 상태를 추적하는 변수
    private Vector3 offset; // 오브젝트와 마우스 위치 사이의 거리

    void Update()
    {
        // 마우스 버튼을 눌렀을 때 드래그 시작
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPosition = GetMouseWorldPosition();

            // Raycast를 사용하여 클릭된 오브젝트 확인
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPosition, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
                offset = transform.position - mouseWorldPosition;
                Debug.Log("Dragging started on: " + gameObject.name); // 드래그 시작 로그 출력
            }
            else
            {
                Debug.Log("No collider hit at mouse position."); // 충돌이 없을 때 로그 출력
            }
        }

        // 마우스 버튼을 뗄 때 드래그 종료
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            Debug.Log("Dragging stopped."); // 드래그 종료 로그 출력
        }

        // 드래그 중일 때 오브젝트 이동
        if (isDragging)
        {
            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            transform.position = mouseWorldPosition + offset; // 마우스 위치에 오프셋을 추가하여 이동
        }
    }

    // 마우스 스크린 좌표를 월드 좌표로 변환하는 함수
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition; // 스크린 좌표에서 마우스 위치 가져오기
        mouseScreenPosition.z = 0f; // 2D 환경에서는 z 값을 0으로 설정
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition); // 월드 좌표로 변환
    }
}
