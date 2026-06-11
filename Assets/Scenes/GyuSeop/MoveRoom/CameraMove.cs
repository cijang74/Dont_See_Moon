using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Cinemachine; // 유니티 6 전용 네임스페이스

public class CameraManager : MonoBehaviour
{
    [Header("카메라 설정")]
    public CinemachineCamera playerCam; // 기본 1인칭 카메라
    public CinemachineCamera objectCam; // 오브젝트 타겟 카메라

    [Header("페이드 설정")]
    public Image fadeImage;
    public float fadeDuration = 0.5f;

    private bool isAtObject = false;
    private bool isTransitioning = false;

    void Start()
    {
        // 시작 시 1인칭 카메라의 우선순위를 높게 설정
        playerCam.Priority = 10;
        objectCam.Priority = 5;
    }

    void Update()
    {
        // 마우스 클릭 시 레이캐스트 (시야가 고정되어도 커서 위치 기준)
        if (Input.GetMouseButtonDown(0) && !isTransitioning)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // 클릭한 대상이 이 스크립트가 붙은 오브젝트라면
                if (hit.transform == this.transform)
                {
                    StartCoroutine(SwitchCameraProcess());
                }
            }
        }
    }

    IEnumerator SwitchCameraProcess()
    {
        isTransitioning = true;

        // 1. 페이드 아웃 (화면이 검게 변함)
        yield return StartCoroutine(Fade(0, 1));

        // 2. 카메라 우선순위 변경 (시네머신 브레인이 알아서 컷 전환)
        if (!isAtObject)
        {
            playerCam.Priority = 5;
            objectCam.Priority = 10;
            isAtObject = true;
        }
        else
        {
            playerCam.Priority = 10;
            objectCam.Priority = 5;
            isAtObject = false;
        }

        // 전환 직후 아주 잠깐 대기하여 안정화
        yield return new WaitForSeconds(0.1f);

        // 3. 페이드 인 (화면이 밝아짐)
        yield return StartCoroutine(Fade(1, 0));

        isTransitioning = false;
    }

    IEnumerator Fade(float start, float end)
    {
        float elapsed = 0;
        Color c = fadeImage.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(start, end, elapsed / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = end;
        fadeImage.color = c;
    }
}