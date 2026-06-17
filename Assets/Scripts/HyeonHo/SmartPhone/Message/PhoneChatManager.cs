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

    // (선택) 생존 판정을 직접 덮어쓰고 싶을 때만 사용.
    // null이면 Rooms.csv의 Character 매핑 + CharacterStatusManager.IsAlive 로 자동 판정.
    public System.Func<string, bool> IsContactAlive;

    [Header("알림음 (새 메시지 도착 시)")]
    [SerializeField] private AudioSource sfxSource;     // 비우면 자동으로 추가됨
    [SerializeField] private AudioClip newMessageSound; // 진동/알림음 클립

    public event System.Action OnRoomsChanged;

    private readonly Dictionary<string, RoomRuntime> rooms = new Dictionary<string, RoomRuntime>();

    private const string SaveKey = "PhoneChatSave";
    private int processedDay;   // 발송 처리까지 끝난 날짜 (세이브됨)
    private int lastCheckedDay; // Update에서 날짜 변화 감지용 (런타임)
    private bool firstDayProcessed; // 로드 직후 첫 확인엔 알림음 생략

    private void Awake()
    {
        Instance = this;

        // 알림음용 AudioSource 준비 (인스펙터에 안 넣으면 자동 추가)
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

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
        bool delivered = false;

        foreach (var r in rooms.Values)
        {
            foreach (var node in r.data.nodes.Values)
            {
                if (node.triggerDay <= 0) continue;            // 예약 안 된 노드
                if (node.triggerDay <= processedDay) continue; // 이미 처리한 날
                if (node.triggerDay > currentDay) continue;    // 아직 안 된 날

                // 발신자(상대 캐릭터)가 사망 상태면 발송하지 않음
                if (!IsSenderAlive(r.data)) continue;

                TriggerNewMessage(r.data.roomId, node.nodeId);
                delivered = true;
            }
        }

        if (currentDay > processedDay)
        {
            processedDay = currentDay;
            SaveProgress();
        }

        // 날짜가 바뀌어 새 메시지가 실제로 도착했을 때만 알림음
        // (게임 시작/로드 직후 현재 날짜까지 따라잡는 첫 확인에는 울리지 않음)
        if (delivered && firstDayProcessed)
            PlayNotifySound();

        firstDayProcessed = true;
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

    // 새 메시지 도착 알림음 재생 (게임 이벤트로 메시지를 보낼 때도 직접 호출 가능)
    public void PlayNotifySound()
    {
        if (newMessageSound != null && sfxSource != null)
            sfxSource.PlayOneShot(newMessageSound);
    }

    // 방의 상대 캐릭터가 생존해 있는지 (사망 시 메시지 발송 안 함)
    private bool IsSenderAlive(ChatRoom room)
    {
        // 직접 주입한 판정이 있으면 우선 사용
        if (IsContactAlive != null) return IsContactAlive(room.roomId);

        // 캐릭터가 매핑된 방이면 CharacterStatusManager로 생존 확인
        if (room.hasCharacter && CharacterStatusManager.Instance != null)
            return CharacterStatusManager.Instance.IsAlive(room.characterType);

        // 매핑이 없거나 매니저가 없으면 생존으로 간주(항상 발송)
        return true;
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
        firstDayProcessed = false; // 리셋 후 재발송은 알림음 없이 조용히

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