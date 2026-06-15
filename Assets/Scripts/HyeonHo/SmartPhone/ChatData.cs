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
    public List<string> messages = new List<string>();        // NPC가 보내는 메시지(순서대로)
    public List<ChatChoice> choices = new List<ChatChoice>(); // 답변 선택지
    public bool IsEnd => choices.Count == 0;                  // 선택지가 없으면 종료 노드
}

// 하나의 채팅방(대화 상대)
public class ChatRoom
{
    public string roomId;
    public string displayName; // 목록 표시 이름 (엄마, 아빠 ...)
    public string startNodeId; // 시작 노드 ID
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

// CSV(TextAsset) → ChatRoom 데이터로 변환
// 구분자는 쉼표가 아니라 파이프(|) 입니다. 대화 텍스트에 쉼표가 들어가도 충돌하지 않습니다.
public static class ChatCsvLoader
{
    // 구분자 — 대화 텍스트에 절대 쓰지 않을 문자
    private const char Delimiter = '|';
    private static readonly string DelimiterStr = Delimiter.ToString();

    // Rooms : RoomId|DisplayName|StartNodeId
    // Chats : RoomId|NodeId|Type|NextNodeId|Text   (Text는 항상 마지막 칸)
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
            if (row.Length < 5 || string.IsNullOrWhiteSpace(row[0])) continue;

            string roomId = row[0].Trim();
            if (!rooms.TryGetValue(roomId, out var room)) continue;

            string nodeId = row[1].Trim();
            if (!room.nodes.TryGetValue(nodeId, out var node))
            {
                node = new ChatNode { nodeId = nodeId };
                room.nodes[nodeId] = node;
            }

            string type = row[2].Trim().ToLowerInvariant();
            string nextNodeId = row[3].Trim();
            // 5번째 칸부터 끝까지를 모두 Text로 합침 (혹시 텍스트에 | 가 있어도 복원)
            string text = JoinFrom(row, 4);

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

    // 줄 단위로 나눈 뒤 각 줄을 구분자로 분리 (따옴표 처리 불필요 — 구분자가 텍스트에 안 나옴)
    static List<string[]> ParseLines(string text)
    {
        var rows = new List<string[]>();
        text = text.Replace("\r\n", "\n").Replace("\r", "\n");

        foreach (var line in text.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line)) continue; // 빈 줄 무시
            rows.Add(line.Split(Delimiter));
        }
        return rows;
    }
}