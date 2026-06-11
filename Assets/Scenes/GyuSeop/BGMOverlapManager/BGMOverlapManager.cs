using UnityEngine;

public class BGMCrossfadeManager : MonoBehaviour
{
    [Header("BGM Settings")]
    public AudioClip bgmClip;
    public float overlapTime = 2.0f; 
    [Range(0f, 1f)] public float maxVolume = 1.0f;

    [HideInInspector] public float masterMultiplier = 1.0f;

    private AudioSource[] sources = new AudioSource[2];
    private int currentIndex = 0;             
    private double currentFadeStartTime;      
    private bool isScheduled = false;         
    private bool isFading = false;            

    void Start()
    {
        for (int i = 0; i < 2; i++)
        {
            sources[i] = gameObject.AddComponent<AudioSource>();
            sources[i].clip = bgmClip;
            sources[i].loop = false; 
            sources[i].playOnAwake = false;
            sources[i].volume = 0f; 
        }

        if (bgmClip != null)
        {
            double nextStartTime = AudioSettings.dspTime + 0.1f;
            sources[currentIndex].volume = maxVolume * masterMultiplier; 
            sources[currentIndex].PlayScheduled(nextStartTime);
            currentFadeStartTime = nextStartTime + (bgmClip.length - overlapTime);
        }
    }

    void Update()
    {
        if (bgmClip == null) return;

        double currentTime = AudioSettings.dspTime;
        int nextIndex = 1 - currentIndex;

        if (currentTime + 1.0f > currentFadeStartTime && !isScheduled)
        {
            sources[nextIndex].volume = 0f;
            sources[nextIndex].PlayScheduled(currentFadeStartTime);
            isScheduled = true;
        }

        float targetCurrentVol = maxVolume;
        float targetNextVol = 0f;

        if (currentTime >= currentFadeStartTime)
        {
            if (isScheduled)
            {
                isScheduled = false;
                isFading = true;
            }

            if (isFading)
            {
                float progress = (float)((currentTime - currentFadeStartTime) / overlapTime);
                if (progress >= 1.0f)
                {
                    progress = 1.0f;
                    isFading = false;
                    
                    sources[currentIndex].Stop(); 
                    
                    currentIndex = nextIndex;
                    currentFadeStartTime += (bgmClip.length - overlapTime);
                    
                    targetCurrentVol = maxVolume;
                    targetNextVol = 0f;
                }
                else
                {
                    float angle = progress * Mathf.PI / 2f;
                    targetNextVol = Mathf.Sin(angle) * maxVolume;
                    targetCurrentVol = Mathf.Cos(angle) * maxVolume;
                }
            }
        }

        sources[currentIndex].volume = targetCurrentVol * masterMultiplier;
        if (isFading || isScheduled)
        {
            sources[nextIndex].volume = targetNextVol * masterMultiplier;
        }
    }

    // --- [신규 추가] 외부에서 BGM을 안전하게 강제 교체하는 함수 ---
    public void ChangeBGM(AudioClip newClip)
    {
        // 지정된 새 오디오가 없거나, 이미 재생 중인 곡과 같다면 교체하지 않음
        if (newClip == null || bgmClip == newClip) return;

        bgmClip = newClip;

        // 1. 기존에 돌고 있던 모든 오디오 소스를 즉시 정지 (충돌 방지)
        sources[0].Stop();
        sources[1].Stop();

        // 2. 루프 관련 제어 변수들 깨끗하게 리셋
        currentIndex = 0;
        isScheduled = false;
        isFading = false;

        // 3. 0번 소스에 새 클립을 장착하고 즉시 재생 예약
        sources[currentIndex].clip = bgmClip;
        
        // 현재 암전 상태이므로 볼륨은 0인 상태로 시작함 (이후 페이드인 되면서 소리가 커짐)
        sources[currentIndex].volume = maxVolume * masterMultiplier; 
        
        double nextStartTime = AudioSettings.dspTime + 0.05f; // 미세한 대기 후 재생
        sources[currentIndex].PlayScheduled(nextStartTime);

        // 4. 새 BGM에 맞는 다음 루프 시간 재계산
        currentFadeStartTime = nextStartTime + (bgmClip.length - overlapTime);
        
        Debug.Log($"[BGM Manager] BGM이 새로운 클립으로 안전하게 교체되었습니다: {newClip.name}");
    }
}