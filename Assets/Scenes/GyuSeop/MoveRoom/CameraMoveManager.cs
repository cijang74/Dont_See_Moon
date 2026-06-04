using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Cinemachine;

public class CameraMoveManager : MonoBehaviour
{
    [Header("페이드 설정")]
    public Image fadeImage;
    public float fadeDuration = 0.4f;

    private CinemachineCamera currentActiveCam;
    private CinemachineCamera previousActiveCam; 
    private bool isTransitioning = false;

    void Start()
    {
        currentActiveCam = FindFirstActiveCamera();
        if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, 0);
    }

    void Update()
    {
        if (isTransitioning) return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscape();
        }
    }

    void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            CameraNode node = hit.transform.GetComponent<CameraNode>();

            if (node != null && node.targetCamera != null && node.targetCamera != currentActiveCam)
            {
                // 클릭해서 이동할 때는 일반 전환(isGoBack = false)
                StartCoroutine(SwitchCamera(node.targetCamera, false));
            }
        }
    }

    void HandleEscape()
    {
        if (previousActiveCam != null && previousActiveCam != currentActiveCam)
        {
            // ESC로 돌아갈 때는 뒤로 가기 전환(isGoBack = true)
            StartCoroutine(SwitchCamera(previousActiveCam, true));
        }
    }

    // isGoBack 매개변수를 추가하여 클릭 이동과 ESC 복귀를 구분합니다.
    IEnumerator SwitchCamera(CinemachineCamera newCam, bool isGoBack)
    {
        isTransitioning = true;

        // 1. 페이드 아웃
        yield return StartCoroutine(Fade(0, 1));

        // 2. 이전 카메라 기록 처리
        if (!isGoBack)
        {
            // 클릭해서 새 카메라로 갈 때는 현재 카메라를 이전 카메라로 기억
            previousActiveCam = currentActiveCam;
        }
        else
        {
            // ESC로 돌아갈 때는 더 이상 돌아갈 곳이 없도록 기록을 초기화
            previousActiveCam = null;
        }

        // 우선순위 교체
        if (currentActiveCam != null) currentActiveCam.Priority = 5;
        
        newCam.Priority = 10;
        currentActiveCam = newCam; 

        yield return new WaitForSeconds(0.1f);

        // 3. 페이드 인
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

    private CinemachineCamera FindFirstActiveCamera()
    {
        var cams = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        CinemachineCamera highest = null;
        int maxPriority = -1;

        foreach (var c in cams)
        {
            if (c.Priority > maxPriority)
            {
                maxPriority = c.Priority;
                highest = c;
            }
        }
        return highest;
    }
}