using UnityEngine;
using UnityEngine.UI; // Button
using TMPro;          // TextMeshProUGUI

// 뉴스 목록의 버튼 하나 (제목 + 빨간 배지)
public class NewsButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private GameObject unreadBadge;
    [SerializeField] private Button button;

    private NewsRuntime item;

    public void Setup(NewsRuntime item, System.Action<string> onClick)
    {
        this.item = item;

        if (titleText != null) titleText.text = item.data.title;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke(item.data.newsId));
        }

        RefreshBadge();
    }

    // 등장한 뉴스만 목록에 표시하고, 읽지 않았으면 배지 표시
    public void RefreshBadge()
    {
        if (item == null) return;

        gameObject.SetActive(item.arrived);                 // 등장 날짜 전이면 목록에서 숨김
        if (unreadBadge != null) unreadBadge.SetActive(item.hasUnread);
    }
}