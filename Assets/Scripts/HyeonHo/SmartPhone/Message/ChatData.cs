using System.Collections.Generic;
using UnityEngine;

// 메시지 발신 주체
public enum ChatSender { Npc, Player }

// 플레이어 답변 선택지 한 개
public class ChatChoice
{
    public string text;       // 버튼에 표시될 텍스트
    public string nextNodeId; // 선택 시 이동할 노드 ("" 또는 END = 대화 종료)
}

// 대화 트리의 한 노드: NPC 메시지 묶음 + 답변 선택지 묶음
public class ChatNode
{
    public string nodeId;
    public int triggerDay = 0;                                // 0이면 예약 없음. >0이면 그 날짜에 발송될 시작 노드
    public List<string> messages = new List<string>();        // NPC가 보내는 메시지(순서대로)
    public List<ChatChoice> choices = new List<ChatChoice>(); // 답변 선택지
    public bool IsEnd => choices.Count == 0;                  // 선택지가 없으면 종료 노드
}

// 하나의 채팅방(대화 상대)
public class ChatRoom
{
    public string roomId;
    public string displayName; // 목록 표시 이름 (엄마, 아빠 ...)
    public string startNodeId; // (선택) 참고용 시작 노드. 실제 발송은 Day 예약으로 동작
    public Dictionary<string, ChatNode> nodes = new Dictionary<string, ChatNode>();

    public ChatNode GetNode(string id)
    {
        if (!string.IsNullOrEmpty(id) && nodes.TryGetValue(id, out var n)) return n;
        return null;
    }
}

// 화면에 표시됐던 한 줄(다시 열었을 때 / 세이브 복원용)
public class DisplayedLine
{
    public ChatSender sender;
    public string text;
}

// 런타임 상태(진행 위치, 배지 여부, 기록)
public class RoomRuntime
{
    public ChatRoom data;
    public string currentNodeId;
    public bool awaitingChoice; // 현재 노드 메시지를 다 보냈고 답변 대기 중
    public bool completed;      // 대화 완료
    public bool hasUnread;      // 빨간 배지 표시 여부
    public readonly List<DisplayedLine> history = new List<DisplayedLine>();
}

// CSV(TextAsset) → ChatRoom 데이터로 변환. 구분자는 파이프(|).
public static class ChatCsvLoader
{
    private const char Delimiter = '|';
    private static readonly string DelimiterStr = Delimiter.ToString();

    // Rooms : RoomId|DisplayName|StartNodeId
    // Chats : RoomId|NodeId|Type|Day|NextNodeId|Text   (Text는 항상 마지막 칸)
    public static Dictionary<string, ChatRoom> Load(TextAsset roomsCsv, TextAsset chatsCsv)
    {
        var rooms = new Dictionary<string, ChatRoom>();
        if (roomsCsv == null || chatsCsv == null)
        {
            Debug.LogError("ChatCsvLoader: CSV TextAsset이 할당되지 않았습니다.");
            return rooms;
        }

        // 1) 방 목록
        var roomRows = ParseLines(roomsCsv.text);
        for (int r = 1; r < roomRows.Count; r++) // 0행은 헤더
        {
            var row = roomRows[r];
            if (row.Length < 3 || string.IsNullOrWhiteSpace(row[0])) continue;
            var room = new ChatRoom
            {
                roomId = row[0].Trim(),
                displayName = row[1].Trim(),
                startNodeId = row[2].Trim()
            };
            rooms[room.roomId] = room;
        }

        // 2) 대화 노드
        var chatRows = ParseLines(chatsCsv.text);
        for (int r = 1; r < chatRows.Count; r++)
        {
            var row = chatRows[r];
            if (row.Length < 6 || string.IsNullOrWhiteSpace(row[0])) continue;

            string roomId = row[0].Trim();
            if (!rooms.TryGetValue(roomId, out var room)) continue;

            string nodeId = row[1].Trim();
            if (!room.nodes.TryGetValue(nodeId, out var node))
            {
                node = new ChatNode { nodeId = nodeId };
                room.nodes[nodeId] = node;
            }

            string type = row[2].Trim().ToLowerInvariant();
            string dayStr = row[3].Trim();
            string nextNodeId = row[4].Trim();
            // 6번째 칸부터 끝까지를 모두 Text로 합침 (콤마가 들어가도 안전)
            string text = JoinFrom(row, 5);

            // Day가 적힌 노드는 해당 날짜에 발송되는 시작 노드로 표시
            if (!string.IsNullOrEmpty(dayStr) && int.TryParse(dayStr, out int day))
                node.triggerDay = day;

            if (type == "msg" || type == "message")
            {
                node.messages.Add(text);
            }
            else if (type == "choice")
            {
                node.choices.Add(new ChatChoice { text = text, nextNodeId = nextNodeId });
            }
        }

        return rooms;
    }

    static string JoinFrom(string[] arr, int start)
    {
        if (arr.Length <= start) return "";
        return string.Join(DelimiterStr, arr, start, arr.Length - start);
    }

    static List<string[]> ParseLines(string text)
    {
        var rows = new List<string[]>();
        text = text.Replace("\r\n", "\n").Replace("\r", "\n");

        foreach (var line in text.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            rows.Add(line.Split(Delimiter));
        }
        return rows;
    }
}