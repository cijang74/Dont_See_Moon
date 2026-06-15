using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro; // TextMeshPro 사용
using Unity.Cinemachine;

public class DayTransitionManager : MonoBehaviour
{
    // 외부 스크립트(CameraMoveManager)에서 연출 중인지 확인할 수 있는 전역 변수
    public static bool IsTransitioning = false;

    [Header("UI 연결")]
    [Tooltip("CameraMoveManager에 연결된 것과 동일한 검은색 페이드 이미지를 연결하세요.")]
    public Image fadeImage;
    [Tooltip("화면 중앙에 배치된 DAY 표시용 TextMeshPro UI를 연결하세요.")]
    public TextMeshProUGUI dayText;

    [Header("매니저 연결")]
    public CameraMoveManager camManager;
    public BGMCrossfadeManager bgmManager;

    [Header("날짜 및 다음 씬 설정")]
    public int currentDay = 1;
    [Tooltip("다음 날짜가 시작될 때 바라볼 카메라 시점")]
    public CinemachineCamera nextDayStartCamera;
    [Tooltip("다음 날짜가 시작될 때 재생될 기본 BGM")]
    public AudioClip nextDayBGM;

    [Header("시간 설정 (초)")]
    public float fadeOutToBlackDuration = 1.5f; // 화면 까매지는 시간
    public float textFadeInDuration = 2.0f;     // DAY 글씨 나타나는 시간
    public float holdFirstDayTime = 1.5f;       // 글씨 유지 시간
    // ★ [추가됨] 이전 날짜가 사라지고 다음 날짜가 나타날 때까지의 공백 시간
    public float delayBetweenDays = 0.5f;
    public float holdSecondDayTime = 1.0f;      // 바뀐 날짜 유지 시간
    // ★ [추가됨] 코루틴 안에 있던 변수를 인스펙터에서 조절할 수 있게 위로 뺐습니다.
    public float textFadeOutDuration = 0.8f;
    public float fadeInToSceneDuration = 1.5f;  // 최종적으로 씬이 밝아지는 시간

    void Start()
    {
        // 시작할 때 텍스트를 투명하게 초기화
        if (dayText != null)
        {
            Color c = dayText.color;
            c.a = 0f;
            dayText.color = c;
        }
    }

    void Update()
    {
        // 이벤트 시스템이 아직 없으므로 임시로 P키를 눌러 작동
        if (Input.GetKeyDown(KeyCode.P) && !IsTransitioning)
        {
            StartCoroutine(DayTransitionRoutine());
        }
    }

    IEnumerator DayTransitionRoutine()
    {
        IsTransitioning = true;

        // 1. 화면 검게 페이드 인 & BGM 볼륨 0으로 페이드 아웃
        float elapsed = 0;
        Color fadeColor = fadeImage.color;
        float startBgmVolume = bgmManager.masterMultiplier;

        while (elapsed < fadeOutToBlackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutToBlackDuration;
            
            fadeColor.a = Mathf.Lerp(0, 1, t);
            fadeImage.color = fadeColor;
            
            bgmManager.masterMultiplier = Mathf.Lerp(startBgmVolume, 0, t);
            yield return null;
        }
        
        fadeColor.a = 1;
        fadeImage.color = fadeColor;
        bgmManager.masterMultiplier = 0;

        // -----------------------------------------------------
        // 이 시점에서 화면은 완벽한 암전 상태, 사운드는 0입니다.
        // 2. 카메라 시점 변경 및 BGM 트랙 교체 (유저 눈에는 안 보임)
        // -----------------------------------------------------
        if (nextDayStartCamera != null) camManager.ForceSetCameraAndClearHistory(nextDayStartCamera);
        if (nextDayBGM != null) bgmManager.ChangeBGM(nextDayBGM);

        // 3. DAY N 글씨 나타나기 & BGM 볼륨 다시 1로 페이드 인
        dayText.text = $"DAY {currentDay}";
        Color textColor = dayText.color;
        elapsed = 0;

        while (elapsed < textFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / textFadeInDuration;
            
            textColor.a = Mathf.Lerp(0, 1, t);
            dayText.color = textColor;
            
            bgmManager.masterMultiplier = Mathf.Lerp(0, 1, t);
            yield return null;
        }

        textColor.a = 1;
        dayText.color = textColor;
        bgmManager.masterMultiplier = 1;

        // 4. DAY N 유지
        yield return new WaitForSeconds(holdFirstDayTime);

        // 5. 글씨 꺼지듯 깜빡! (순간적으로 사라짐)
        textColor.a = 0;
        dayText.color = textColor;
        // ★ [수정됨] 하드코딩된 0.4초 대신 인스펙터 변수 적용
        yield return new WaitForSeconds(delayBetweenDays);

        // 6. 다음 날짜로 바뀌어서 나타남 (순간적으로 나타남)
        currentDay++;
        dayText.text = $"DAY {currentDay}";
        textColor.a = 1;
        dayText.color = textColor;
        
        yield return new WaitForSeconds(holdSecondDayTime);

        // 7. 글씨가 먼저 부드럽게 페이드 아웃 (검은 화면 유지)
        elapsed = 0;
        // ★ [수정됨] 위로 변수를 뺐으므로 float 선언(float textFadeOutDuration = 0.8f;)을 삭제했습니다.
        while (elapsed < textFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / textFadeOutDuration;
            
            textColor.a = Mathf.Lerp(1, 0, t);
            dayText.color = textColor;
            yield return null;
        }
        textColor.a = 0;
        dayText.color = textColor;

        // 글씨가 사라진 후 아주 잠깐의 여운 (선택 사항)
        yield return new WaitForSeconds(0.3f); 

        // 8. 그 다음 검은 화면이 페이드 아웃되며 다음 날짜 씬 시작
        elapsed = 0;
        while (elapsed < fadeInToSceneDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInToSceneDuration;
            
            fadeColor.a = Mathf.Lerp(1, 0, t);
            fadeImage.color = fadeColor;
            yield return null;
        }

        fadeColor.a = 0;
        fadeImage.color = fadeColor;

        // 연출 완전 종료
        IsTransitioning = false;
    }
}