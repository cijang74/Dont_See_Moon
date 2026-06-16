using System.Collections.Generic;
using UnityEngine;

// 모든 채팅방의 데이터/런타임 상태를 관리하고, 날짜에 맞춰 메시지를 발송하며, 진행도를 저장/복원하는 싱글턴
public class PhoneChatManager : MonoBehaviour
{
    public static PhoneChatManager Instance { get; private set; }

    [Header("CSV (Assets 내 .csv 파일을 드래그)")]
    [SerializeField] private TextAsset roomsCsv;
    [SerializeField] private TextAsset chatsCsv;

    [Header("날짜 연동")]
    [Tooltip("currentDay를 읽어올 DayTransitionManager를 연결하세요.")]
    [SerializeField] private DayTransitionManager dayManager;

    // 외부에서 주입: roomId → 발신자 생존 여부(true=생존). null이면 항상 생존으로 간주.
    // ★ 사망 판단 소스가 정해지면 이 델리게이트에 연결하면 됩니다. 예:
    //   PhoneChatManager.Instance.IsContactAlive = id => CharacterManager.IsAlive(id);
    public System.Func<string, bool> IsContactAlive;

    public event System.Action OnRoomsChanged;

    private readonly Dictionary<string, RoomRuntime> rooms = new Dictionary<string, RoomRuntime>();

    private const string SaveKey = "PhoneChatSave";
    private int processedDay;   // 발송 처리까지 끝난 날짜 (세이브됨)
    private int lastCheckedDay; // Update에서 날짜 변화 감지용 (런타임)

    private void Awake()
    {
        Instance = this;
        Load();
        LoadProgress(); // 세이브가 있으면 덮어씀
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Load()
    {
        var data = ChatCsvLoader.Load(roomsCsv, chatsCsv);
        rooms.Clear();
        foreach (var kv in data)
        {
            rooms[kv.Key] = new RoomRuntime
            {
                data = kv.Value,
                currentNodeId = "",   // 아직 도착한 대화 없음 (Day 예약으로 시작됨)
                hasUnread = false
            };
        }
    }

    // 날짜가 바뀔 때만 확인 (DayTransitionManager.currentDay 변화 감지)
    private void Update()
    {
        if (dayManager == null) return;

        if (dayManager.currentDay != lastCheckedDay)
        {
            lastCheckedDay = dayManager.currentDay;
            ProcessDay(dayManager.currentDay);
        }
    }

    // 현재 날짜까지 예약된 메시지를 발송 (이미 지난 날은 건너뜀, 사망자는 발송 안 함)
    private void ProcessDay(int currentDay)
    {
        foreach (var r in rooms.Values)
        {
            foreach (var node in r.data.nodes.Values)
            {
                if (node.triggerDay <= 0) continue;            // 예약 안 된 노드
                if (node.triggerDay <= processedDay) continue; // 이미 처리한 날
                if (node.triggerDay > currentDay) continue;    // 아직 안 된 날

                // 발신자가 사망 상태면 발송하지 않음
                if (IsContactAlive != null && !IsContactAlive(r.data.roomId)) continue;

                TriggerNewMessage(r.data.roomId, node.nodeId);
            }
        }

        if (currentDay > processedDay)
        {
            processedDay = currentDay;
            SaveProgress();
        }
    }

    public IEnumerable<RoomRuntime> GetRooms() => rooms.Values;

    public RoomRuntime GetRoom(string id)
        => rooms.TryGetValue(id, out var r) ? r : null;

    // 읽지 않은 메시지가 하나라도 있으면 true (홈 화면 Message 앱 배지용)
    public bool HasAnyUnread()
    {
        foreach (var r in rooms.Values)
            if (r.hasUnread) return true;
        return false;
    }

    // 특정 방에 새 메시지가 도착했음을 알림 (Day 예약 또는 게임 이벤트에서 호출)
    public void TriggerNewMessage(string roomId, string fromNodeId = null)
    {
        var r = GetRoom(roomId);
        if (r == null) return;

        if (!string.IsNullOrEmpty(fromNodeId))
        {
            r.currentNodeId = fromNodeId;
            r.awaitingChoice = false;
            r.completed = false;
        }
        r.hasUnread = true;
        OnRoomsChanged?.Invoke();
        SaveProgress();
    }

    // 대화가 끝나면 호출 → 배지 제거
    public void MarkCompleted(string roomId)
    {
        var r = GetRoom(roomId);
        if (r == null) return;

        r.completed = true;
        r.awaitingChoice = false;
        r.hasUnread = false;
        OnRoomsChanged?.Invoke();
        SaveProgress();
    }

    // ─────────────── 진행도 저장/복원 (PlayerPrefs + JSON) ───────────────

    [System.Serializable]
    private class LineSave { public int sender; public string text; }

    [System.Serializable]
    private class RoomSave
    {
        public string roomId;
        public string currentNodeId;
        public bool awaitingChoice;
        public bool completed;
        public bool hasUnread;
        public List<LineSave> history = new List<LineSave>();
    }

    [System.Serializable]
    private class ChatSaveData
    {
        public int processedDay;
        public List<RoomSave> rooms = new List<RoomSave>();
    }

    public void SaveProgress()
    {
        var data = new ChatSaveData { processedDay = processedDay };
        foreach (var r in rooms.Values)
        {
            var rs = new RoomSave
            {
                roomId = r.data.roomId,
                currentNodeId = r.currentNodeId,
                awaitingChoice = r.awaitingChoice,
                completed = r.completed,
                hasUnread = r.hasUnread
            };
            foreach (var l in r.history)
                rs.history.Add(new LineSave { sender = (int)l.sender, text = l.text });
            data.rooms.Add(rs);
        }

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return;
        var json = PlayerPrefs.GetString(SaveKey);
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<ChatSaveData>(json);
        if (data?.rooms == null) return;

        processedDay = data.processedDay;

        foreach (var rs in data.rooms)
        {
            if (!rooms.TryGetValue(rs.roomId, out var r)) continue;

            r.currentNodeId = rs.currentNodeId;
            r.awaitingChoice = rs.awaitingChoice;
            r.completed = rs.completed;
            r.hasUnread = rs.hasUnread;

            r.history.Clear();
            foreach (var ls in rs.history)
                r.history.Add(new DisplayedLine { sender = (ChatSender)ls.sender, text = ls.text });
        }
    }

    // 테스트용: 저장된 진행도/날짜 초기화 (다음 실행부터 처음 상태)
    public void ClearProgress()
    {
        PlayerPrefs.DeleteKey(SaveKey);
    }

    // 테스트용: 지금 즉시 모든 대화 진행도를 초기화하고 화면을 갱신.
    // (UI 버튼의 onClick에 연결해서 사용)
    public void ResetAll()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        processedDay = 0;
        lastCheckedDay = 0; // 다음 Update에서 현재 날짜까지 예약 메시지를 다시 발송

        // RoomRuntime 객체는 그대로 두고 내용만 비움 (버튼/뷰의 참조가 유지되도록)
        foreach (var r in rooms.Values)
        {
            r.currentNodeId = "";
            r.awaitingChoice = false;
            r.completed = false;
            r.hasUnread = false;
            r.history.Clear();
        }

        OnRoomsChanged?.Invoke(); // 목록 배지 즉시 갱신
    }
}