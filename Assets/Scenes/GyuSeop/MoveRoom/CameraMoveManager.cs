using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Cinemachine;

public class CameraMoveManager : MonoBehaviour
{
    [Header("페이드 세부 설정")]
    public Image fadeImage;
    [Tooltip("화면이 완전히 어두워질 때까지 걸리는 시간 (초)")]
    public float fadeOutDuration = 0.4f;
    [Tooltip("화면이 완전히 어두워진 채로 유지되는 시간 (초)")]
    public float holdDuration = 0.5f;
    [Tooltip("화면이 다시 원래대로 밝아질 때까지 걸리는 시간 (초)")]
    public float fadeInDuration = 0.4f;
    
    [Header("사운드 연동")]
    public BGMCrossfadeManager bgmManager; 

    [Header("전환 효과음 설정 (글로벌 마스터 스위치)")]
    [Tooltip("체크를 해제하면 모든 오브젝트 전환 시 효과음이 전면 차단됩니다.")]
    public bool playTransitionSFX = true;
    [Tooltip("기본 문 닫히는 효과음")]
    public AudioClip defaultCloseSFX;
    [Tooltip("기본 문 열리는 효과음")]
    public AudioClip defaultOpenSFX;

    private CinemachineCamera currentActiveCam;
    private CinemachineCamera previousActiveCam; 
    private AudioClip previousBgmClip; 
    
    // --- [신규 추가] 이전 노드의 효과음 설정을 기억할 백업 변수들 ---
    private bool previousUseSFX = true;
    private AudioClip previousCloseSFX;
    private AudioClip previousOpenSFX;
    
    private AudioSource sfxSource;
    private bool isTransitioning = false;

    void Start()
    {
        currentActiveCam = FindFirstActiveCamera();
        if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, 0);

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
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
                StartCoroutine(SwitchCamera(node.targetCamera, node.newBgmClip, node.customCloseSFX, node.customOpenSFX, node.useTransitionSFX, false));
            }
        }
    }

    void HandleEscape()
    {
        if (previousActiveCam != null && previousActiveCam != currentActiveCam)
        {
            // ★ [핵심 수정] ESC로 돌아갈 때, 백업해 두었던 이전 노드의 효과음 설정을 그대로 매개변수로 전달합니다.
            StartCoroutine(SwitchCamera(previousActiveCam, previousBgmClip, previousCloseSFX, previousOpenSFX, previousUseSFX, true));
        }
    }

    IEnumerator SwitchCamera(CinemachineCamera newCam, AudioClip newBgm, AudioClip closeSFX, AudioClip openSFX, bool nodeUseSFX, bool isGoBack)
    {
        isTransitioning = true;

        // 글로벌 스위치와 현재 적용할 노드의 스위치가 모두 켜져 있어야 효과음이 재생됨
        bool shouldPlaySFX = playTransitionSFX && nodeUseSFX;

        // 1. 페이드 아웃 시작 시점에 '열리는 소리(openSFX)' 재생
        if (shouldPlaySFX)
        {
            AudioClip clipToPlay = openSFX != null ? openSFX : defaultOpenSFX;
            if (clipToPlay != null) sfxSource.PlayOneShot(clipToPlay);
        }

        // 페이드 아웃 진행
        yield return StartCoroutine(Fade(0, 1, fadeOutDuration));

        // 2. 암전 상태 데이터 처리
        if (!isGoBack)
        {
            // 전진할 때: 현재 카메라와 BGM 백업
            previousActiveCam = currentActiveCam;
            previousBgmClip = bgmManager != null ? bgmManager.bgmClip : null;

            // ★ [핵심 수정] 전진할 때 클릭했던 노드의 효과음 설정값들도 함께 백업 변수에 저장
            previousUseSFX = nodeUseSFX;
            previousCloseSFX = closeSFX;
            previousOpenSFX = openSFX;
        }

        // BGM 변경 적용
        if (bgmManager != null && newBgm != null)
        {
            bgmManager.ChangeBGM(newBgm);
        }

        if (isGoBack)
            {
            // 복귀 완료 후에는 다음 복귀를 위해 모든 백업 데이터 초기화
            previousActiveCam = null;
            previousBgmClip = null;
            previousUseSFX = true;
            previousCloseSFX = null;
            previousOpenSFX = null;
        }

        // 카메라 우선순위 교체
        if (currentActiveCam != null) currentActiveCam.Priority = 5;
        newCam.Priority = 10;
        currentActiveCam = newCam; 

        // 암전 유지 시간
        yield return new WaitForSeconds(holdDuration);

        // 3. 암전이 끝나고 페이드 인이 시작되는 시점에 '닫히는 소리(closeSFX)' 재생
        if (shouldPlaySFX)
        {
            AudioClip clipToPlay = closeSFX != null ? closeSFX : defaultCloseSFX;
            if (clipToPlay != null) sfxSource.PlayOneShot(clipToPlay);
        }

        // 페이드 인 진행 (화면이 밝아짐)
        yield return StartCoroutine(Fade(1, 0, fadeInDuration));

        isTransitioning = false;
    }

    IEnumerator Fade(float start, float end, float duration)
    {
        float elapsed = 0;
        Color c = fadeImage.color;
        
        float startVolumeMult = 1f - start; 
        float endVolumeMult = 1f - end;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

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