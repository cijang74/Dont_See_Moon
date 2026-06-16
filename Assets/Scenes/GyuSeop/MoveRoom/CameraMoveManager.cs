using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;

public class CameraMoveManager : MonoBehaviour
{
    [System.Serializable]
    public class CameraHistoryData
    {
        public CinemachineCamera camera;
        public bool useSFX;
        public AudioClip closeSFX;
        public AudioClip openSFX;
        public bool wasSmooth;

        public CameraHistoryData(CinemachineCamera camera, bool useSFX, AudioClip closeSFX, AudioClip openSFX, bool wasSmooth)
        {
            this.camera = camera;
            this.useSFX = useSFX;
            this.closeSFX = closeSFX;
            this.openSFX = openSFX;
            this.wasSmooth = wasSmooth;
        }
    }

    [Header("페이드 세부 설정")]
    public Image fadeImage;
    public float fadeOutDuration = 0.4f;
    public float holdDuration = 0.5f;
    public float fadeInDuration = 0.4f;

    [Header("스무스 이동 설정")]
    [Tooltip("부드럽게 이동할 때 걸리는 시간")]
    public float smoothBlendDuration = 2f; 
    
    [Header("사운드 연동")]
    public BGMCrossfadeManager bgmManager; 

    [Header("전환 효과음 설정")]
    public bool playTransitionSFX = true;
    public AudioClip defaultCloseSFX;
    public AudioClip defaultOpenSFX;

    private CinemachineCamera currentActiveCam;
    private CinemachineBrain mainBrain; 
    private int currentHierarchyLevel = 0; 
    private List<CameraHistoryData> cameraHistory = new List<CameraHistoryData>();
    private AudioSource sfxSource;
    private bool isTransitioning = false;

    void Start()
    {
        if (Camera.main != null)
        {
            mainBrain = Camera.main.GetComponent<CinemachineBrain>();
        }

        currentActiveCam = FindFirstActiveCamera();
        
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

        if (SmartphoneUI.IsActive) return;

        // ★ [추가됨] 날짜 전환 연출 중일 때 카메라 이동 입력을 막습니다.
        if (DayTransitionManager.IsTransitioning) return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleGoBack();
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
                CameraSettings camSettings = node.targetCamera.GetComponent<CameraSettings>();
                int targetTier = camSettings != null ? camSettings.hierarchyLevel : 0;
                AudioClip targetBgm = camSettings != null ? camSettings.bgmClip : null;

                StartCoroutine(SwitchCamera(node.targetCamera, targetBgm, node.customCloseSFX, node.customOpenSFX, node.useTransitionSFX, targetTier, false, node.isSmoothTransition));
            }
        }
    }

    void HandleGoBack()
    {
        if (cameraHistory.Count > 0)
        {
            int lastIndex = cameraHistory.Count - 1;
            CameraHistoryData lastState = cameraHistory[lastIndex];
            
            cameraHistory.RemoveAt(lastIndex);

            CameraSettings camSettings = lastState.camera.GetComponent<CameraSettings>();
            int targetTier = camSettings != null ? camSettings.hierarchyLevel : 0;
            AudioClip targetBgm = camSettings != null ? camSettings.bgmClip : null;

            StartCoroutine(SwitchCamera(lastState.camera, targetBgm, lastState.closeSFX, lastState.openSFX, lastState.useSFX, targetTier, true, lastState.wasSmooth));
        }
    }

    IEnumerator SwitchCamera(CinemachineCamera newCam, AudioClip newBgm, AudioClip closeSFX, AudioClip openSFX, bool nodeUseSFX, int targetHierarchyLevel, bool isGoBack, bool isSmooth)
    {
        isTransitioning = true;
        bool shouldPlaySFX = playTransitionSFX && nodeUseSFX;

        // ★ [수정됨] 커스텀 오디오가 비어있으면 디폴트 오디오를 재생하도록 설정
        if (shouldPlaySFX)
        {
            AudioClip clipToPlay = openSFX != null ? openSFX : defaultOpenSFX;
            if (clipToPlay != null) sfxSource.PlayOneShot(clipToPlay);
        }

        if (!isSmooth)
        {
            yield return StartCoroutine(Fade(0, 1, fadeOutDuration));
        }

        if (!isGoBack)
        {
            cameraHistory.RemoveAll(h => h.camera == currentActiveCam);

            if (targetHierarchyLevel > currentHierarchyLevel)
            {
                cameraHistory.Add(new CameraHistoryData(currentActiveCam, nodeUseSFX, closeSFX, openSFX, isSmooth));
            }
            else
            {
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

        if (mainBrain != null)
        {
            if (isSmooth)
            {
                mainBrain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseOut, smoothBlendDuration);
            }
            else
            {
                mainBrain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
            }
        }

        CameraSettings oldSettings = null;
        if (currentActiveCam != null) 
        {
            currentActiveCam.Priority = 5;
            oldSettings = currentActiveCam.GetComponent<CameraSettings>();
            if (oldSettings != null) oldSettings.onCameraExit?.Invoke();
        }
        
        newCam.Priority = 10;
        currentActiveCam = newCam; 

        CameraSettings newSettings = newCam.GetComponent<CameraSettings>();
        if (newSettings != null) newSettings.onCameraEnter?.Invoke();

        currentHierarchyLevel = targetHierarchyLevel;

        if (isSmooth)
        {
            yield return new WaitForSeconds(smoothBlendDuration);

            // ★ [추가됨] 스무스 이동이 완전히 끝난 시점에도 종료 효과음이 나도록 처리
            if (shouldPlaySFX)
            {
                AudioClip clipToPlay = closeSFX != null ? closeSFX : defaultCloseSFX;
                if (clipToPlay != null) sfxSource.PlayOneShot(clipToPlay);
            }
            
            if (oldSettings != null) oldSettings.onCameraExitComplete?.Invoke();
            if (newSettings != null) newSettings.onCameraEnterComplete?.Invoke();
        }
        else
        {
            yield return new WaitForSeconds(holdDuration);
            
            if (shouldPlaySFX)
            {
                AudioClip clipToPlay = closeSFX != null ? closeSFX : defaultCloseSFX;
                if (clipToPlay != null) sfxSource.PlayOneShot(clipToPlay);
            }

            yield return StartCoroutine(Fade(1, 0, fadeInDuration));
            
            if (oldSettings != null) oldSettings.onCameraExitComplete?.Invoke();
            if (newSettings != null) newSettings.onCameraEnterComplete?.Invoke();
        }

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

    // ★ [신규 추가] 날짜가 넘어가면서 강제로 씬의 시작 시점을 바꾸고 히스토리를 비우는 함수
    public void ForceSetCameraAndClearHistory(CinemachineCamera newCam)
    {
        if (newCam == null) return;

        CameraSettings oldSettings = null;
        // 기존 카메라 끄기
        if (currentActiveCam != null) 
        {
            currentActiveCam.Priority = 5;
            oldSettings = currentActiveCam.GetComponent<CameraSettings>();
            if (oldSettings != null) oldSettings.onCameraExit?.Invoke();
        }
        
        // 새 카메라(다음 날짜의 시작 시점) 켜기
        newCam.Priority = 10;
        currentActiveCam = newCam;

        CameraSettings newSettings = newCam.GetComponent<CameraSettings>();
        if (newSettings != null) newSettings.onCameraEnter?.Invoke();

        if (oldSettings != null) oldSettings.onCameraExitComplete?.Invoke();
        if (newSettings != null) newSettings.onCameraEnterComplete?.Invoke();

        // 방문 기록 및 계층 레벨 완전 초기화
        cameraHistory.Clear();
        currentHierarchyLevel = 0; 
    }
}