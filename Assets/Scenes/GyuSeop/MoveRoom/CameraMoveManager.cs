using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;

public class CameraMoveManager : MonoBehaviour
{
    // [변경점] BGM과 계층 정보는 카메라 자체가 들고 있으므로, 히스토리에는 전환 효과음 정보만 저장합니다.
    [System.Serializable]
    public class CameraHistoryData
    {
        public CinemachineCamera camera;
        public bool useSFX;
        public AudioClip closeSFX;
        public AudioClip openSFX;

        public CameraHistoryData(CinemachineCamera camera, bool useSFX, AudioClip closeSFX, AudioClip openSFX)
        {
            this.camera = camera;
            this.useSFX = useSFX;
            this.closeSFX = closeSFX;
            this.openSFX = openSFX;
        }
    }

    [Header("페이드 세부 설정")]
    public Image fadeImage;
    public float fadeOutDuration = 0.4f;
    public float holdDuration = 0.5f;
    public float fadeInDuration = 0.4f;
    
    [Header("사운드 연동")]
    public BGMCrossfadeManager bgmManager; 

    [Header("전환 효과음 설정")]
    public bool playTransitionSFX = true;
    public AudioClip defaultCloseSFX;
    public AudioClip defaultOpenSFX;

    private CinemachineCamera currentActiveCam;
    private int currentHierarchyLevel = 0; 
    private List<CameraHistoryData> cameraHistory = new List<CameraHistoryData>();
    private AudioSource sfxSource;
    private bool isTransitioning = false;

    void Start()
    {
        currentActiveCam = FindFirstActiveCamera();
        
        // 시작 카메라의 계층 정보 초기화 (CameraSettings가 없으면 0으로 간주)
        if (currentActiveCam != null)
        {
            CameraSettings camSettings = currentActiveCam.GetComponent<CameraSettings>();
            currentHierarchyLevel = camSettings != null ? camSettings.hierarchyLevel : 0;
        }

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
                // ★ 노드가 아닌, '이동할 타겟 카메라'에서 정보(BGM, 계층)를 가져옵니다.
                CameraSettings camSettings = node.targetCamera.GetComponent<CameraSettings>();
                int targetTier = camSettings != null ? camSettings.hierarchyLevel : 0;
                AudioClip targetBgm = camSettings != null ? camSettings.bgmClip : null;

                StartCoroutine(SwitchCamera(node.targetCamera, targetBgm, node.customCloseSFX, node.customOpenSFX, node.useTransitionSFX, targetTier, false));
            }
        }
    }

    void HandleEscape()
    {
        if (cameraHistory.Count > 0)
        {
            int lastIndex = cameraHistory.Count - 1;
            CameraHistoryData lastState = cameraHistory[lastIndex];
            
            cameraHistory.RemoveAt(lastIndex);

            // ★ 복귀할 때도 '복귀 대상 카메라'에서 정보(BGM, 계층)를 가져옵니다.
            CameraSettings camSettings = lastState.camera.GetComponent<CameraSettings>();
            int targetTier = camSettings != null ? camSettings.hierarchyLevel : 0;
            AudioClip targetBgm = camSettings != null ? camSettings.bgmClip : null;

            StartCoroutine(SwitchCamera(lastState.camera, targetBgm, lastState.closeSFX, lastState.openSFX, lastState.useSFX, targetTier, true));
        }
    }

    IEnumerator SwitchCamera(CinemachineCamera newCam, AudioClip newBgm, AudioClip closeSFX, AudioClip openSFX, bool nodeUseSFX, int targetHierarchyLevel, bool isGoBack)
    {
        isTransitioning = true;
        bool shouldPlaySFX = playTransitionSFX && nodeUseSFX;

        if (shouldPlaySFX)
        {
            AudioClip clipToPlay = openSFX != null ? openSFX : defaultOpenSFX;
            if (clipToPlay != null) sfxSource.PlayOneShot(clipToPlay);
        }

        yield return StartCoroutine(Fade(0, 1, fadeOutDuration));

        if (!isGoBack)
        {
            cameraHistory.RemoveAll(h => h.camera == currentActiveCam);

            if (targetHierarchyLevel > currentHierarchyLevel)
            {
                cameraHistory.Add(new CameraHistoryData(currentActiveCam, nodeUseSFX, closeSFX, openSFX));
            }
            else
            {
                // ★ 히스토리 내의 카메라들을 순회하며, 해당 카메라의 CameraSettings를 직접 검사하여 하위 계층 기록을 삭제합니다.
                cameraHistory.RemoveAll(h => {
                    CameraSettings settings = h.camera.GetComponent<CameraSettings>();
                    int hLevel = settings != null ? settings.hierarchyLevel : 0;
                    return hLevel >= targetHierarchyLevel;
                });
            }
        }

        if (bgmManager != null && newBgm != null)
        {
            bgmManager.ChangeBGM(newBgm);
        }

        if (currentActiveCam != null) currentActiveCam.Priority = 5;
        newCam.Priority = 10;
        currentActiveCam = newCam; 

        currentHierarchyLevel = targetHierarchyLevel;

        yield return new WaitForSeconds(holdDuration);

        if (shouldPlaySFX)
        {
            AudioClip clipToPlay = closeSFX != null ? closeSFX : defaultCloseSFX;
            if (clipToPlay != null) sfxSource.PlayOneShot(clipToPlay);
        }

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

            if (bgmManager != null) bgmManager.masterMultiplier = Mathf.Lerp(startVolumeMult, endVolumeMult, t);
            yield return null;
        }
        
        c.a = end;
        fadeImage.color = c;
        if (bgmManager != null) bgmManager.masterMultiplier = endVolumeMult;
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