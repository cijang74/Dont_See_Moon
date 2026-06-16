using System.Collections.Generic;
using UnityEngine;

// 뉴스 데이터/상태를 관리하고, 날짜에 맞춰 뉴스를 등장시키며, 진행도를 저장/복원하는 싱글턴
public class NewsManager : MonoBehaviour
{
    public static NewsManager Instance { get; private set; }

    [Header("CSV (Assets 내 .csv 파일을 드래그)")]
    [SerializeField] private TextAsset newsRoomCsv;
    [SerializeField] private TextAsset newsChatsCsv;

    [Header("날짜 연동")]
    [SerializeField] private DayTransitionManager dayManager;

    public event System.Action OnNewsChanged;

    private readonly Dictionary<string, NewsRuntime> items = new Dictionary<string, NewsRuntime>();

    private const string SaveKey = "PhoneNewsSave";
    private int processedDay;
    private int lastCheckedDay;

    private void Awake()
    {
        Instance = this;
        Load();
        LoadProgress();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Load()
    {
        var data = NewsCsvLoader.Load(newsRoomCsv, newsChatsCsv);
        items.Clear();
        foreach (var kv in data)
            items[kv.Key] = new NewsRuntime { data = kv.Value, hasUnread = false, read = false };
    }

    // 날짜가 바뀔 때만 확인
    private void Update()
    {
        if (dayManager == null) return;
        if (dayManager.currentDay != lastCheckedDay)
        {
            lastCheckedDay = dayManager.currentDay;
            ProcessDay(dayManager.currentDay);
        }
    }

    // 현재 날짜까지 예약된 뉴스를 등장시킴
    private void ProcessDay(int currentDay)
    {
        foreach (var it in items.Values)
        {
            if (it.data.triggerDay <= 0) continue;
            if (it.data.triggerDay <= processedDay) continue;
            if (it.data.triggerDay > currentDay) continue;

            it.hasUnread = true;
            it.read = false;
        }

        if (currentDay > processedDay)
            processedDay = currentDay;

        OnNewsChanged?.Invoke();
        SaveProgress();
    }

    public IEnumerable<NewsRuntime> GetItems() => items.Values;
    public NewsRuntime GetItem(string id) => items.TryGetValue(id, out var i) ? i : null;

    // 뉴스를 열어서 읽으면 호출 → 배지 제거
    public void MarkRead(string id)
    {
        var it = GetItem(id);
        if (it == null) return;

        it.read = true;
        it.hasUnread = false;
        OnNewsChanged?.Invoke();
        SaveProgress();
    }

    // 읽지 않은 뉴스가 하나라도 있으면 true (홈 화면 News 앱 배지용)
    public bool HasAnyUnread()
    {
        foreach (var it in items.Values)
            if (it.hasUnread) return true;
        return false;
    }

    // ─────────────── 저장/복원 ───────────────

    [System.Serializable]
    private class ItemSave { public string newsId; public bool hasUnread; public bool read; }

    [System.Serializable]
    private class SaveData
    {
        public int processedDay;
        public List<ItemSave> items = new List<ItemSave>();
    }

    public void SaveProgress()
    {
        var data = new SaveData { processedDay = processedDay };
        foreach (var it in items.Values)
            data.items.Add(new ItemSave { newsId = it.data.newsId, hasUnread = it.hasUnread, read = it.read });

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return;
        var json = PlayerPrefs.GetString(SaveKey);
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<SaveData>(json);
        if (data?.items == null) return;

        processedDay = data.processedDay;
        foreach (var s in data.items)
        {
            if (!items.TryGetValue(s.newsId, out var it)) continue;
            it.hasUnread = s.hasUnread;
            it.read = s.read;
        }
    }

    // 테스트용: 지금 즉시 초기화하고 화면 갱신 (리셋 버튼에 연결 가능)
    public void ResetAll()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        processedDay = 0;
        lastCheckedDay = 0; // 다음 Update에서 현재 날짜까지 다시 등장

        foreach (var it in items.Values)
        {
            it.hasUnread = false;
            it.read = false;
        }

        OnNewsChanged?.Invoke();
    }
}