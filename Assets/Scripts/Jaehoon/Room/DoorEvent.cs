using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorEvent : MonoBehaviour
{
    private FadeSystem fadeScript;

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

    //이동 중인지 돌아오는 중인지 확인하기 위함
    private bool moving = true;
    private bool returning = false;

    private bool isZooming = false;

    void Awake()
    {
        fadeScript = GameObject.Find("EventSystem").GetComponent<FadeSystem>();
    }

    void Start()
    {
        // 초기 위치 저장
        if (cameraTransform != null)
        {
            initialCameraPosition = cameraTransform.position;
        }

        // 초기 속도 설정
        currentSpeed = initialSpeed;

    }

    void Update()
    {
        // 카메라가 이동 중일 경우
        if (isMoving)
        {
            MoveCamera(targetCameraPosition, ref isMoving, moving);
        }

        // 카메라가 초기 위치로 돌아가는 경우
        if (isReturning)
        {
            MoveCamera(initialCameraPosition, ref isReturning, returning);
        }

        // ESC 키를 눌렀을 때 초기 위치로 돌아가기 시작
        if (Input.GetKeyDown(KeyCode.Escape) && !isMoving && isZooming)
        {
            StartCoroutine(ReturnCamera());
        }
    }

    void OnMouseDown()
    {
        // 객체가 클릭되었을 때 이동 시작
        if (cameraTransform != null && !isMoving && !isReturning)
        {
            isMoving = true;
            Debug.Log("카메라 이동을 시작합니다.");
        }
        else
        {
            Debug.LogWarning("카메라가 이미 이동 중입니다.");
        }
    }

    private void MoveCamera(Vector3 destination, ref bool stateFlag, bool state)
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
            
            if(state)
            {
                StartCoroutine(Fade(0f, 0.99f, 0.5f, moving));
            }
        }
    }

    private IEnumerator ReturnCamera()
    {
        StartCoroutine(Fade(0f, 0.99f, 0.5f, returning));

        yield return new WaitForSeconds(0.5f);

        isReturning = true;
        isZooming = false;
        Debug.Log("카메라가 초기 위치로 돌아갑니다.");
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float fadeTime, bool state)
    {
        //화면 암전시키기기
        StartCoroutine(fadeScript.FadeIn(startAlpha, endAlpha, fadeTime));
        
        yield return new WaitForSeconds(fadeTime);

        if(state)
        {
            ChangeScene("outDoor");
        }
        else
        {
            ChangeScene("inDoor");
        }

        //화면 다시 밝게 만들기
        StartCoroutine(fadeScript.FadeIn(endAlpha, startAlpha, fadeTime));

        isZooming = true;
    }

    private void ChangeScene(string scene)
    {
        if(scene == "outDoor"){
            //씬을 기존 씬 위에 덧붙여서 로드
            SceneManager.LoadScene("OutDoorScene", LoadSceneMode.Additive);
        }
        else if(scene == "inDoor"){
            //덧붙힌 씬 닫기
            SceneManager.UnloadSceneAsync("OutDoorScene");
        }
    }
}
