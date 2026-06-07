using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Cinemachine;

public class CameraMoveManager : MonoBehaviour
{
    [Header("페이드 설정")]
    public Image fadeImage;
    public float fadeDuration = 0.4f;
    
    [Header("사운드 연동")]
    public BGMCrossfadeManager bgmManager; 

    private CinemachineCamera currentActiveCam;
    private CinemachineCamera previousActiveCam; 
    
    // 이전 장소에서 재생 중이던 BGM을 기억할 변수 추가
    private AudioClip previousBgmClip; 
    
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
                // 클릭해서 전진할 때는 노드에 설정된 새 BGM을 넘겨줍니다.
                StartCoroutine(SwitchCamera(node.targetCamera, node.newBgmClip, false));
            }
        }
    }

    void HandleEscape()
    {
        if (previousActiveCam != null && previousActiveCam != currentActiveCam)
        {
            // ★ ESC로 돌아갈 때는 저장해두었던 '이전 BGM 클립'을 매개변수로 직접 넘겨줍니다.
            StartCoroutine(SwitchCamera(previousActiveCam, previousBgmClip, true));
        }
    }

    IEnumerator SwitchCamera(CinemachineCamera newCam, AudioClip newBgm, bool isGoBack)
    {
        isTransitioning = true;

        // 1. 페이드 아웃 (화면이 완전히 검은색이 될 때까지 대기)
        yield return StartCoroutine(Fade(0, 1));

        // 2. [핵심] 암전 상태에서 기록 저장 및 BGM 교체 처리
        if (!isGoBack)
        {
            // 클릭해서 새 장소로 이동하기 직전, '현재 카메라'와 '현재 재생 중인 BGM'을 이전 기록으로 백업
            previousActiveCam = currentActiveCam;
            previousBgmClip = bgmManager != null ? bgmManager.bgmClip : null;
        }

        // BGM 변경 적용 (새 장소의 BGM이거나, ESC 복귀 시 이전 장소의 BGM)
        if (bgmManager != null && newBgm != null)
        {
            bgmManager.ChangeBGM(newBgm);
        }

        if (isGoBack)
        {
            // 복귀 완료 후에는 더 이상 돌아갈 곳이 없도록 기록 초기화
            previousActiveCam = null;
            previousBgmClip = null;
        }

        // 3. 카메라 우선순위 교체
        if (currentActiveCam != null) currentActiveCam.Priority = 5;
        
        newCam.Priority = 10;
        currentActiveCam = newCam; 

        yield return new WaitForSeconds(0.1f);

        // 4. 페이드 인 (새로운 혹은 복원된 BGM이 화면과 함께 밝아짐)
        yield return StartCoroutine(Fade(1, 0));

        isTransitioning = false;
    }

    IEnumerator Fade(float start, float end)
    {
        float elapsed = 0;
        Color c = fadeImage.color;
        
        float startVolumeMult = 1f - start; 
        float endVolumeMult = 1f - end;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            c.a = Mathf.Lerp(start, end, t);
            fadeImage.color = c;

            if (bgmManager != null)
            {
                bgmManager.masterMultiplier = Mathf.Lerp(startVolumeMult, endVolumeMult, t);
            }

            yield return null;
        }
        
        c.a = end;
        fadeImage.color = c;
        if (bgmManager != null)
        {
            bgmManager.masterMultiplier = endVolumeMult;
        }
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