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
    private bool isTransitioning = false;

    void Start()
    {
        // 씬 시작 시 가장 높은 우선순위를 가진 카메라를 현재 카메라로 설정
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
    }

    void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 클릭한 오브젝트에서 CameraNode 컴포넌트를 찾음
            CameraNode node = hit.transform.GetComponent<CameraNode>();

            // 노드가 존재하고, 그 노드에 설정된 카메라가 현재 카메라와 다를 때만 실행
            if (node != null && node.targetCamera != null && node.targetCamera != currentActiveCam)
            {
                StartCoroutine(SwitchCamera(node.targetCamera));
            }
        }
    }

    IEnumerator SwitchCamera(CinemachineCamera newCam)
    {
        isTransitioning = true;

        // 1. 페이드 아웃
        yield return StartCoroutine(Fade(0, 1));

        // 2. 우선순위 교체 (현재 카메라는 낮게, 새 카메라는 높게)
        if (currentActiveCam != null) currentActiveCam.Priority = 5;
        
        newCam.Priority = 10;
        currentActiveCam = newCam;

        // 시네머신 브레인이 전환될 시간을 짧게 대기
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