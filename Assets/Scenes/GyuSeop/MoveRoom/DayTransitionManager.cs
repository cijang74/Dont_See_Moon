using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using TMPro; // TextMeshPro 사용
using Unity.Cinemachine;

[System.Serializable]
public class DailyEventInfo
{
    [Tooltip("해당 날짜가 시작될 때 1회 실행할 기능들")]
    public UnityEvent onDayStartEvent;
}

[System.Serializable]
public class NPCObjectMapping
{
    public InteractionObjectType npcType;
    public GameObject npcObject;
    [Tooltip("해당 NPC 사망 시 함께 비활성화할 추가 오브젝트 1")]
    public GameObject additionalObject1;
    [Tooltip("해당 NPC 사망 시 함께 비활성화할 추가 오브젝트 2")]
    public GameObject additionalObject2;
    [Tooltip("해당 NPC 사망 시 함께 비활성화할 추가 오브젝트 3")]
    public GameObject additionalObject3;
}

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

    [Header("날짜별 자동 실행 이벤트")]
    [Tooltip("리스트의 순서가 곧 날짜입니다. (Element 0 = 1일차, Element 1 = 2일차...)\n기본 1~7일차까지 준비되어 있으며, +버튼으로 계속 늘릴 수 있습니다.")]
    public List<DailyEventInfo> dailyEvents = new List<DailyEventInfo> 
    {
        new DailyEventInfo(), new DailyEventInfo(), new DailyEventInfo(), 
        new DailyEventInfo(), new DailyEventInfo(), new DailyEventInfo(), new DailyEventInfo()
    };

    [Header("NPC 오브젝트 관리")]
    [Tooltip("각 NPC 타입과 씬에 배치된 실제 게임 오브젝트를 연결해주세요.")]
    public List<NPCObjectMapping> npcList = new List<NPCObjectMapping>();

    void OnValidate()
    {
        // 에디터 상에서 리스트가 비어있을 경우 자동으로 7일차 슬롯 생성
        if (dailyEvents == null || dailyEvents.Count == 0)
        {
            dailyEvents = new List<DailyEventInfo> 
            {
                new DailyEventInfo(), new DailyEventInfo(), new DailyEventInfo(), 
                new DailyEventInfo(), new DailyEventInfo(), new DailyEventInfo(), new DailyEventInfo()
            };
        }
    }

    void Start()
    {
        // 시작할 때 텍스트를 투명하게 초기화
        if (dayText != null)
        {
            Color c = dayText.color;
            c.a = 0f;
            dayText.color = c;
        }

        // 게임 시작 시(보통 1일차) 해당 날짜 이벤트 1회 자동 실행
        TriggerEventsForDay(currentDay);

        // 💡 게임 시작 시점에도 죽은 NPC 관련 오브젝트들을 비활성화 처리 (데이터 불러오기 등 대비)
        DeactivateDeadNPCObjects();

        // 첫 시작(또는 씬 진입) 연출 실행
        StartCoroutine(InitialStartRoutine());
    }

    void Update()
    {
        // [임시 기능 주석 처리] 이제 P키가 아닌 특정 오브젝트를 클릭하여 날짜를 넘깁니다.
        // if (Input.GetKeyDown(KeyCode.P) && !IsTransitioning)
        // {
        //     TriggerTransition();
        // }
    }

    /// <summary>
    /// 외부 스크립트(또는 클릭 트리거 오브젝트)에서 날짜 전환 연출을 시작할 때 호출하는 메서드입니다.
    /// </summary>
    public void TriggerTransition()
    {
        if (!IsTransitioning)
        {
            StartCoroutine(DayTransitionRoutine());
        }
    }

    IEnumerator InitialStartRoutine()
    {
        IsTransitioning = true;

        // 1. 처음 화면은 완전한 검은색
        if (fadeImage != null)
        {
            Color fc = fadeImage.color;
            fc.a = 1f;
            fadeImage.color = fc;
        }

        // 2. 현재 날짜 세팅
        if (dayText != null)
        {
            dayText.text = $"DAY {currentDay}";
        }

        // 3. DAY 글씨 페이드 인
        float elapsed = 0;
        Color textColor = dayText.color;
        
        while (elapsed < textFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / textFadeInDuration;
            
            textColor.a = Mathf.Lerp(0, 1, t);
            dayText.color = textColor;
            yield return null;
        }
        textColor.a = 1;
        dayText.color = textColor;

        // 4. 글씨 유지 (기존 머무르는 시간 변수 활용)
        yield return new WaitForSeconds(holdSecondDayTime);

        // 5. 글씨 페이드 아웃
        elapsed = 0;
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

        // 글씨가 사라진 후 아주 잠깐의 여운
        yield return new WaitForSeconds(0.3f); 

        // 6. 검은 화면 페이드 아웃 (씬 시작)
        elapsed = 0;
        Color fadeColor = fadeImage.color;
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

        IsTransitioning = false;
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

        // 💡 화면이 까매졌을 때 투표 결과 집계 및 처형 처리
        VoteManager.Instance.CalculateDailyVoteResults();

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

        // 💡 [추가] 날짜가 바뀌었으므로 DialogueManager의 대화 기록을 초기화!
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ResetDailyInteractions();
        }

        WorkManager.Instance.isWorkToday = false;
        
        // ★ 날짜가 바뀌었으므로 새 날짜에 등록된 이벤트들 자동 실행!
        TriggerEventsForDay(currentDay);

        // 💡 [추가] 이벤트에서 오브젝트를 켰을 수도 있으므로, 죽은 NPC 관련 오브젝트는 다시 확실히 끕니다.
        DeactivateDeadNPCObjects();

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

        // 💡 [추가] 연출이 끝나고 화면이 완전히 밝아진 직후 엔딩 검사 실행!
        // 여기서 엔딩이 트리거되면 EndingManager가 바로 Scene을 교체해버립니다.
        if (EndingManager.Instance != null)
        {
            bool isEndingTriggered = EndingManager.Instance.CheckAndTriggerEnding(currentDay);
            
            // 엔딩이 시작되었다면 더 이상 일반적인 조작을 막기 위해 IsTransitioning을 풀지 않고 대기
            if (isEndingTriggered)
            {
                yield break; // 코루틴 즉시 종료
            }
        }

        // 연출 완전 종료
        IsTransitioning = false;
    }

    /// <summary>
    /// 지정된 날짜에 등록된 이벤트가 있다면 모두 실행합니다.
    /// (1일차는 인덱스 0, 2일차는 인덱스 1에 해당)
    /// </summary>
    private void TriggerEventsForDay(int day)
    {
        if (dailyEvents == null) return;

        int index = day - 1;
        if (index >= 0 && index < dailyEvents.Count)
        {
            dailyEvents[index].onDayStartEvent?.Invoke();
        }
    }

    /// <summary>
    /// NPC의 생존 여부를 확인하여 사망한 NPC와 관련된 모든 오브젝트를 비활성화합니다.
    /// </summary>
    private void DeactivateDeadNPCObjects()
    {
        if (npcList == null) return;

        foreach (var npc in npcList)
        {
            if (CharacterStatusManager.Instance != null && !CharacterStatusManager.Instance.IsAlive(npc.npcType))
            {
                if (npc.npcObject != null) npc.npcObject.SetActive(false);
                if (npc.additionalObject1 != null) npc.additionalObject1.SetActive(false);
                if (npc.additionalObject2 != null) npc.additionalObject2.SetActive(false);
                if (npc.additionalObject3 != null) npc.additionalObject3.SetActive(false);
            }
        }
    }
}