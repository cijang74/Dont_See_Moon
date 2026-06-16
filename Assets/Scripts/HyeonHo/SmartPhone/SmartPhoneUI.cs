using UnityEngine;
using TMPro; // [추가] DAY 텍스트(TMP) 출력용

// 스마트폰 화면을 아래에서 올려 가운데 정지시키고, F로 토글(다시 누르면 내림)
// + 종횡비 고정 동적 크기, + 활성 상태를 정적으로 노출해 맵 상호작용 차단에 사용
// + [추가] DayTransitionManager.currentDay 값을 폰 화면 텍스트에 "Day N"으로 출력
[RequireComponent(typeof(CanvasGroup))]
public class SmartphoneUI : MonoBehaviour
{
    // ── 외부에서 "폰이 떠 있는가"를 확인하기 위한 정적 접근 ──
    public static SmartphoneUI Instance { get; private set; }
    // 숨김 상태가 아니면(올라오는 중/정지/내려가는 중) 활성으로 간주
    public static bool IsActive => Instance != null && Instance.state != PhoneState.Hidden;

    // 완전히 열렸을 때 / 닫혔을 때 외부에서 후처리 하고 싶을 때 사용 (커서 잠금 해제 등)
    public event System.Action OnOpened;
    public event System.Action OnClosed;

    [Header("참조")]
    [SerializeField] private RectTransform phoneRect; // 비우면 자기 자신

    // ───────────── [추가] 날짜 표시 ─────────────
    [Header("날짜 표시 (Day N)")]
    [Tooltip("SmartPhone 안의 날짜 표시용 TMP 텍스트를 연결하세요.")]
    [SerializeField] private TextMeshProUGUI dayText;
    [Tooltip("{0} 자리에 일차 숫자가 들어갑니다. 예: \"Day {0}\" → Day 1")]
    [SerializeField] private string dayFormat = "Day {0}";

    private DayTransitionManager dayManager; // 날짜를 보관하는 매니저(씬에서 자동 탐색)
    private int lastDay = int.MinValue;      // 값이 바뀔 때만 텍스트를 갱신하기 위한 캐시
    // ────────────────────────────────────────────

    [Header("종횡비 (가로 : 세로)")]
    [SerializeField] private float aspectWidth = 2f;
    [SerializeField] private float aspectHeight = 3f;

    [Header("최대 크기 비율 (이 영역 안에 맞춤)")]
    [SerializeField] private float maxWidthRatio = 1f / 3f;
    [SerializeField] private float maxHeightRatio = 8f / 9f;

    [Header("이동 설정")]
    [SerializeField] private float slideSpeed = 4000f;
    [SerializeField] private float arriveThreshold = 1f;

    [Header("입력 키")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F;       // F: 열기/닫기 토글
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;   // ESC: 닫기 전용(선택)
    [SerializeField] private bool useCloseKey = true;             // ESC 닫기 사용 여부

    private CanvasGroup canvasGroup;
    private RectTransform parentRect;
    private Vector2 shownPosition;
    private Vector2 hiddenPosition;
    private Vector2 lastScreenSize;

    private enum PhoneState { Hidden, SlidingUp, Shown, SlidingDown }
    private PhoneState state = PhoneState.Hidden;

    private void Awake()
    {
        Instance = this;

        // 한/영(IME)이 한글 모드일 때 F 같은 키가 조합용으로 가로채여 인식되지 않는 문제 방지.
        // 이 게임은 직접 타이핑이 없으므로 IME 조합을 꺼도 안전.
        Input.imeCompositionMode = IMECompositionMode.Off;

        if (phoneRect == null) phoneRect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        parentRect = phoneRect.parent as RectTransform;

        phoneRect.anchorMin = new Vector2(0.5f, 0.5f);
        phoneRect.anchorMax = new Vector2(0.5f, 0.5f);
        phoneRect.pivot     = new Vector2(0.5f, 0.5f);

        ApplyResponsiveLayout();
        phoneRect.anchoredPosition = hiddenPosition;
        SetInteractable(false);

        // [추가] 씬에 있는 DayTransitionManager 탐색
        dayManager = FindObjectOfType<DayTransitionManager>();
        if (dayManager == null)
            Debug.LogWarning("SmartphoneUI: 씬에서 DayTransitionManager를 찾지 못했습니다. Day 표시가 갱신되지 않습니다.");
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // 종횡비를 유지하면서 최대 영역 안에 들어가는 가장 큰 크기 + 슬라이드 위치 재계산
    private void ApplyResponsiveLayout()
    {
        float canvasW = parentRect.rect.width;
        float canvasH = parentRect.rect.height;

        float targetAspect = aspectWidth / aspectHeight;
        float maxW = canvasW * maxWidthRatio;
        float maxH = canvasH * maxHeightRatio;

        float phoneH = maxH;
        float phoneW = phoneH * targetAspect;
        if (phoneW > maxW)
        {
            phoneW = maxW;
            phoneH = phoneW / targetAspect;
        }

        phoneRect.sizeDelta = new Vector2(phoneW, phoneH);

        shownPosition = Vector2.zero;
        hiddenPosition = new Vector2(0f, -(canvasH * 0.5f + phoneH * 0.5f));

        if (state == PhoneState.Hidden) phoneRect.anchoredPosition = hiddenPosition;
        else if (state == PhoneState.Shown) phoneRect.anchoredPosition = shownPosition;

        lastScreenSize = new Vector2(Screen.width, Screen.height);
    }

    private void Update()
    {
        if (Screen.width != (int)lastScreenSize.x || Screen.height != (int)lastScreenSize.y)
            ApplyResponsiveLayout();

        HandleInput();
        HandleMovement();
        UpdateDayText(); // [추가] 날짜가 바뀌면 자동 반영
    }

    // ───────────── [추가] 날짜 텍스트 갱신 ─────────────
    private void UpdateDayText()
    {
        if (dayManager == null || dayText == null) return;

        int day = dayManager.currentDay; // public int currentDay 직접 접근
        if (day == lastDay) return;      // 값이 그대로면 다시 그리지 않음

        lastDay = day;
        dayText.text = string.Format(dayFormat, day);
    }
    // ────────────────────────────────────────────────

    private void HandleInput()
    {
        // F: 토글
        if (Input.GetKeyDown(toggleKey))
        {
            TogglePhone();
        }
        // ESC: 닫기 전용 (선택)
        else if (useCloseKey && Input.GetKeyDown(closeKey))
        {
            ClosePhone();
        }
    }

    // ── 외부(버튼 등)에서 호출 가능한 공개 메서드 ──

    // 폰 열기 (숨김/내려가는 중일 때만)
    public void OpenPhone()
    {
        if (state == PhoneState.Hidden || state == PhoneState.SlidingDown)
            BeginOpen();
    }

    // 폰 닫기 (정지/올라오는 중일 때만) — X 버튼 onClick에 연결
    public void ClosePhone()
    {
        if (state == PhoneState.Shown || state == PhoneState.SlidingUp)
            BeginClose();
    }

    // 상태에 따라 열고/닫기 — F 키가 호출
    public void TogglePhone()
    {
        if (state == PhoneState.Hidden || state == PhoneState.SlidingDown)
            BeginOpen();
        else
            BeginClose();
    }

    private void BeginOpen()
    {
        state = PhoneState.SlidingUp;
        SetInteractable(false); // 이동 중에는 폰 버튼도 잠시 비활성
    }

    private void BeginClose()
    {
        state = PhoneState.SlidingDown;
        SetInteractable(false);
    }

    private void HandleMovement()
    {
        switch (state)
        {
            case PhoneState.SlidingUp:
                MoveTo(shownPosition);
                if (Reached(shownPosition))
                {
                    phoneRect.anchoredPosition = shownPosition;
                    state = PhoneState.Shown;
                    SetInteractable(true);
                    OnOpened?.Invoke();
                }
                break;

            case PhoneState.SlidingDown:
                MoveTo(hiddenPosition);
                if (Reached(hiddenPosition))
                {
                    phoneRect.anchoredPosition = hiddenPosition;
                    state = PhoneState.Hidden;
                    OnClosed?.Invoke();
                }
                break;
        }
    }

    private void MoveTo(Vector2 target)
    {
        phoneRect.anchoredPosition = Vector2.MoveTowards(
            phoneRect.anchoredPosition, target, slideSpeed * Time.unscaledDeltaTime);
    }

    private bool Reached(Vector2 target)
    {
        return Vector2.Distance(phoneRect.anchoredPosition, target) <= arriveThreshold;
    }

    private void SetInteractable(bool value)
    {
        canvasGroup.interactable = value;
        canvasGroup.blocksRaycasts = value;
    }

    public bool IsShown() => state == PhoneState.Shown;
}