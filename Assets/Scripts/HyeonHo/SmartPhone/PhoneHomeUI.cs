using UnityEngine;

// 홈 화면 ↔ 메시지/뉴스 앱 전환과, 홈의 앱 아이콘 배지를 관리
// SmartPhone(항상 활성) 오브젝트에 붙이는 것을 권장
public class PhoneHomeUI : MonoBehaviour
{
    [Header("화면 오브젝트")]
    [SerializeField] private GameObject homeObject;    // Home
    [SerializeField] private GameObject messageObject; // Message
    [SerializeField] private GameObject newsObject;    // News

    [Header("앱 아이콘 배지 (홈)")]
    [SerializeField] private GameObject messageUnreadBadge; // MessageApp 위 배지
    [SerializeField] private GameObject newsUnreadBadge;    // NewsApp 위 배지

    [Header("(선택) 앱을 열 때 목록부터 보이도록")]
    [SerializeField] private ChatRoomListUI messageListUI;
    [SerializeField] private NewsListUI newsListUI;

    private void Start()
    {
        if (PhoneChatManager.Instance != null) PhoneChatManager.Instance.OnRoomsChanged += RefreshBadges;
        if (NewsManager.Instance != null) NewsManager.Instance.OnNewsChanged += RefreshBadges;
        if (SmartphoneUI.Instance != null) SmartphoneUI.Instance.OnClosed += ShowHome; // 폰을 닫으면 홈으로 초기화

        ShowHome(); // 시작은 홈 화면
    }

    private void OnDestroy()
    {
        if (PhoneChatManager.Instance != null) PhoneChatManager.Instance.OnRoomsChanged -= RefreshBadges;
        if (NewsManager.Instance != null) NewsManager.Instance.OnNewsChanged -= RefreshBadges;
        if (SmartphoneUI.Instance != null) SmartphoneUI.Instance.OnClosed -= ShowHome;
    }

    // ── 앱 버튼 onClick에 연결 ──

    // MessageApp 버튼
    public void OpenMessage()
    {
        if (homeObject != null) homeObject.SetActive(false);
        if (messageObject != null) messageObject.SetActive(true);
        if (newsObject != null) newsObject.SetActive(false);

        if (messageListUI != null) messageListUI.BackToList(); // 항상 목록부터
    }

    // NewsApp 버튼
    public void OpenNews()
    {
        if (homeObject != null) homeObject.SetActive(false);
        if (newsObject != null) newsObject.SetActive(true);
        if (messageObject != null) messageObject.SetActive(false);

        if (newsListUI != null) newsListUI.BackToList(); // 항상 목록부터
    }

    // Message/News의 X 버튼 onClick에 연결 → 홈으로
    public void ShowHome()
    {
        if (homeObject != null) homeObject.SetActive(true);
        if (messageObject != null) messageObject.SetActive(false);
        if (newsObject != null) newsObject.SetActive(false);

        RefreshBadges();
    }

    private void RefreshBadges()
    {
        if (messageUnreadBadge != null)
            messageUnreadBadge.SetActive(PhoneChatManager.Instance != null && PhoneChatManager.Instance.HasAnyUnread());

        if (newsUnreadBadge != null)
            newsUnreadBadge.SetActive(NewsManager.Instance != null && NewsManager.Instance.HasAnyUnread());
    }
}