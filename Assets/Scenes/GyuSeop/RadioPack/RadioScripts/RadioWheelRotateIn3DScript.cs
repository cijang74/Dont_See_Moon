using UnityEngine;

public class RadioWheelRotateIn3DScript : MonoBehaviour
{
    private Vector3 startMousePosition;      // 3D 공간에서의 시작 마우스 위치
    private Vector3 startObjectDirection;   // 오브젝트 초기 방향
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main; // 메인 카메라 가져오기
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 마우스 클릭 시작
        {
            // 마우스 월드 좌표를 구하기 위해 Raycast 사용
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == gameObject) // 마우스 클릭한 오브젝트가 이 오브젝트인지 확인
                {
                    RadioManagerScript.Instance.isDragging = true;

                    startMousePosition = hit.point; // 클릭 시작 위치 저장
                    startObjectDirection = (hit.point - transform.position).normalized; // 초기 방향 계산
                }
            }
        }

        if (Input.GetMouseButton(0) && RadioManagerScript.Instance.isDragging) // 드래그 중
        {
            // 현재 마우스 위치를 Raycast로 계산
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 currentMousePosition = hit.point; // 현재 마우스 위치
                Vector3 currentDirection = (currentMousePosition - transform.position).normalized; // 현재 방향

                // 이전 방향과 현재 방향 사이의 각도 계산
                float angle = Vector3.SignedAngle(startObjectDirection, currentDirection, Vector3.forward);
                transform.Rotate(Vector3.forward, angle); // Y축을 기준으로 회전

                // RadioManager의 라디오 주파수 값 조정
                RadioManagerScript.Instance.radioFrequency -= angle * RadioManagerScript.Instance.ratioChangeSpeed * Time.deltaTime;

                // 현재 상태를 다음 업데이트에 반영
                startMousePosition = currentMousePosition;
                startObjectDirection = currentDirection;
            }
        }

        if (Input.GetMouseButtonUp(0)) // 마우스 클릭 해제
        {
            RadioManagerScript.Instance.isDragging = false;
        }
    }

    private bool IsMouseOnObject(Vector3 mousePosition)
    {
        // IsMouseOnObject는 3D에서 필요하지 않음. Raycast가 오브젝트 히트를 대신 처리.
        return false;
    }
}
