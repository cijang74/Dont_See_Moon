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

                    // 다이얼 평면을 기준으로 방향 계산
                    Plane plane = new Plane(transform.forward, transform.position);
                    if (plane.Raycast(ray, out float enter))
                    {
                        startMousePosition = ray.GetPoint(enter);
                        startObjectDirection = (startMousePosition - transform.position).normalized;
                    }
                }
            }
        }

        if (Input.GetMouseButton(0) && RadioManagerScript.Instance.isDragging) // 드래그 중
        {
            // 물리 레이캐스트 대신 다이얼의 가상 평면을 사용하여 마우스가 오브젝트 밖으로 나가도 회전 가능하게 수정
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(transform.forward, transform.position);

            if (plane.Raycast(ray, out float enter))
            {
                Vector3 currentMousePosition = ray.GetPoint(enter); 
                Vector3 currentDirection = (currentMousePosition - transform.position).normalized;

                // 회전축(transform.forward)을 기준으로 각도 계산산
                float angle = Vector3.SignedAngle(startObjectDirection, currentDirection, transform.forward);
                
                // Z축 회전 적용
                transform.Rotate(Vector3.forward, angle); 

                // RadioManager의 라디오 주파수 값 조정
                RadioManagerScript.Instance.radioFrequency -= angle * RadioManagerScript.Instance.ratioChangeSpeed * Time.deltaTime;

                // 다음 프레임을 위해 현재 방향 저장
                startObjectDirection = currentDirection;
            }
        }

        if (Input.GetMouseButtonUp(0)) // 마우스 클릭 해제
        {
            RadioManagerScript.Instance.isDragging = false;
        }
    }
}
