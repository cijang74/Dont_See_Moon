using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RefrigeratorEvent : MonoBehaviour
{
    // 이동할 카메라의 Transform (드래그 앤 드롭으로 할당)
    [SerializeField] private Transform cameraTransform;

    // 카메라의 목표 위치 값
    [SerializeField] private Vector3 targetCameraPosition;

    // 초기 카메라 위치를 저장
    private Vector3 initialCameraPosition;

    // 초기 이동 속도
    [SerializeField] private float initialSpeed = 0.1f;

    // 최대 이동 속도
    [SerializeField] private float maxSpeed = 5.0f;

    // 가속도
    [SerializeField] private float acceleration = 0.5f;

    // 현재 속도
    private float currentSpeed;

    // 카메라가 이동 중인지 확인
    private bool isMoving = false;

    // 카메라가 돌아오는 중인지 확인
    private bool isReturning = false;

    //지금 냉장고를 확대 중인지 확인
    private bool isZooming = false;



    private Animator animator;

    void Start()
    {
        // 초기 위치 저장
        if (cameraTransform != null)
        {
            initialCameraPosition = cameraTransform.position;
        }

        // 초기 속도 설정
        currentSpeed = initialSpeed;


        //부모의 Animator 가져오기
        animator = transform.parent.GetComponent<Animator>();
    }

    void Update()
    {
        // 카메라가 이동 중일 경우
        if (isMoving)
        {
            MoveCamera(targetCameraPosition, ref isMoving);
        }

        // 카메라가 초기 위치로 돌아가는 경우
        if (isReturning)
        {
            MoveCamera(initialCameraPosition, ref isReturning);
        }

        // ESC 키를 눌렀을 때 초기 위치로 돌아가기 시작
        if (Input.GetKeyDown(KeyCode.Escape) && !isMoving && isZooming)
        {
            animator.SetBool("isOpening", false);
            animator.SetBool("isClosing", true);
            ReturnCamera();
        }
    }

    void OnMouseDown()
    {
        // 객체가 클릭되었을 때 이동 시작
        if (cameraTransform != null && !isMoving && !isReturning)
        {
            isMoving = true;
            Debug.Log("카메라 이동을 시작합니다.");
            animator.SetBool("isClosing", false);
            animator.SetBool("isOpening", true);
            GetComponent<BoxCollider>().enabled = false;
        }
        else
        {
            Debug.LogWarning("카메라가 이미 이동 중입니다.");
        }
    }

    private void MoveCamera(Vector3 destination, ref bool stateFlag)
    {
        // 속도를 점진적으로 증가 (최대 속도를 넘지 않도록 제한)
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);

        // 카메라를 목표 위치로 이동
        cameraTransform.position = Vector3.MoveTowards(cameraTransform.position, destination, currentSpeed * Time.deltaTime);

        // 목표 위치에 도달했는지 확인
        if (Vector3.Distance(cameraTransform.position, destination) < 0.01f)
        {
            cameraTransform.position = destination; // 정확한 위치로 고정
            currentSpeed = initialSpeed; // 속도 초기화
            Debug.Log($"카메라 이동이 완료되었습니다: {destination}");
            stateFlag = false;
            if(!isZooming)
            {
                isZooming = true;
            }
            else
            {
                isZooming = false;
                GetComponent<BoxCollider>().enabled = true;
            }
        }
    }

    private void ReturnCamera()
    {
        isReturning = true;
    }
    
    public bool CheckIsZooming()
    {
        return isZooming;
    }
}
