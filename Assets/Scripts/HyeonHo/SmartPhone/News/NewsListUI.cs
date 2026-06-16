using System.Collections.Generic;
using UnityEngine;

// 뉴스 목록 화면: 뉴스 버튼들을 생성하고, 선택하면 상세 화면으로 전환
public class NewsListUI : MonoBehaviour
{
    [Header("목록")]
    [SerializeField] private Transform listContent;       // 버튼 부모 (Vertical Layout Group 권장)
    [SerializeField] private NewsButton newsButtonPrefab; // 뉴스 버튼 프리팹

    [Header("화면 전환")]
    [SerializeField] private GameObject listPanel; // 목록 패널
    [SerializeField] private GameObject viewPanel; // 상세 패널
    [SerializeField] private NewsView newsView;    // 상세 화면 스크립트

    private readonly List<NewsButton> spawned = new List<NewsButton>();
    private bool built;

    private void Start()
    {
        Build();
    }

    private void OnEnable()
    {
        if (NewsManager.Instance != null)
            NewsManager.Instance.OnNewsChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (NewsManager.Instance != null)
            NewsManager.Instance.OnNewsChanged -= Refresh;
    }

    private void Build()
    {
        if (built || NewsManager.Instance == null) return;

        foreach (var b in spawned) if (b != null) Destroy(b.gameObject);
        spawned.Clear();

        foreach (var it in NewsManager.Instance.GetItems())
        {
            var btn = Instantiate(newsButtonPrefab, listContent);
            btn.Setup(it, OpenNews);
            spawned.Add(btn);
        }
        built = true;
    }

    private void Refresh()
    {
        foreach (var b in spawned) if (b != null) b.RefreshBadge();
    }

    private void OpenNews(string newsId)
    {
        if (listPanel != null) listPanel.SetActive(false);
        if (viewPanel != null) viewPanel.SetActive(true);
        newsView.Open(newsId, BackToList);
    }

    // 상세 화면의 뒤로가기에서 호출
    public void BackToList()
    {
        if (viewPanel != null) viewPanel.SetActive(false);
        if (listPanel != null) listPanel.SetActive(true);
        Refresh();
    }
}