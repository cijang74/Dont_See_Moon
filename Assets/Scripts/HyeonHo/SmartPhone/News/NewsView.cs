using UnityEngine;
using UnityEngine.UI; // Button, ScrollRect
using TMPro;          // TextMeshProUGUI

// 뉴스 상세 화면: 헤더(제목 + 작성 날짜), 본문 스크롤
public class NewsView : MonoBehaviour
{
    [Header("헤더")]
    [SerializeField] private TextMeshProUGUI titleText; // 뉴스 제목
    [SerializeField] private TextMeshProUGUI dateText;  // 작성 날짜
    [SerializeField] private Button backButton;

    [Header("본문")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TextMeshProUGUI bodyText;  // ScrollView Content 안의 본문 TMP

    private System.Action onBack;

    // 목록에서 뉴스를 선택하면 호출
    public void Open(string newsId, System.Action onBack)
    {
        this.onBack = onBack;
        var item = NewsManager.Instance != null ? NewsManager.Instance.GetItem(newsId) : null;
        if (item == null) return;

        if (titleText != null) titleText.text = item.data.title;
        if (dateText != null) dateText.text = item.data.date;
        if (bodyText != null) bodyText.text = string.Join("\n\n", item.data.body); // 문단 사이 한 줄 띄움

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackButton);
        }

        NewsManager.Instance.MarkRead(newsId); // 열면 읽음 처리 → 배지 제거

        ScrollToTop();
    }

    public void OnBackButton()
    {
        onBack?.Invoke();
    }

    private void ScrollToTop()
    {
        if (scrollRect == null) return;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f; // 기사 맨 위부터
    }
}