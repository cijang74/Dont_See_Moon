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

    public void RefreshBadge()
    {
        if (unreadBadge != null && item != null)
            unreadBadge.SetActive(item.hasUnread);
    }
}