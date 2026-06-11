using System.Collections.Generic;
using UnityEngine;

// 모든 채팅방의 데이터/런타임 상태(진행 위치·배지·기록)를 관리하고, 진행도를 저장/복원하는 싱글턴
public class PhoneChatManager : MonoBehaviour
{
    public static PhoneChatManager Instance { get; private set; }

    [Header("CSV (Assets 내 .csv 파일을 드래그)")]
    [SerializeField] private TextAsset roomsCsv;
    [SerializeField] private TextAsset chatsCsv;

    [Header("옵션")]
    [Tooltip("켜면 대화가 있는 모든 방이 시작 시 새 메시지(배지) 상태가 됩니다. (세이브가 있으면 세이브가 우선)")]
    [SerializeField] private bool startAllUnread = true;

    public event System.Action OnRoomsChanged;

    private readonly Dictionary<string, RoomRuntime> rooms = new Dictionary<string, RoomRuntime>();

    private const string SaveKey = "PhoneChatSave";

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
                currentNodeId = kv.Value.startNodeId,
                hasUnread = startAllUnread && kv.Value.nodes.Count > 0
            };
        }
    }

    public IEnumerable<RoomRuntime> GetRooms() => rooms.Values;

    public RoomRuntime GetRoom(string id)
        => rooms.TryGetValue(id, out var r) ? r : null;

    // 게임 이벤트로 특정 방에 새 메시지가 도착했음을 알릴 때 호출
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

    // 해당 방의 대화가 끝나면 호출 → 배지 제거
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
    private class ChatSaveData { public List<RoomSave> rooms = new List<RoomSave>(); }

    // 체크포인트(노드 시작 / 답변 대기 / 완료 시점)에서 호출됨
    public void SaveProgress()
    {
        var data = new ChatSaveData();
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

    // 테스트용: 저장된 진행도 초기화 (다음 실행부터 처음 상태)
    public void ClearProgress()
    {
        PlayerPrefs.DeleteKey(SaveKey);
    }
}