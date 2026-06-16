using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Button, ScrollRect
using TMPro;          // TextMeshProUGUI

// 개별 채팅방 대화 화면: NPC 메시지 재생 → 2개 답변 선택 → 다음 노드 진행
// 메시지 타이밍은 CSV가 아니라 이 스크립트에서 통일 지정. 진행도는 체크포인트마다 저장.
public class ChatRoomView : MonoBehaviour
{
    [Header("타이밍 (모든 메시지 공통)")]
    [SerializeField] private float messageWaitTime = 0.5f; // 메시지 표시 전 대기(초)
    [SerializeField] private float typingTime = 1.0f;      // "입력 중" 표시 지속(초)

    [Header("헤더")]
    [SerializeField] private TextMeshProUGUI headerName;
    [SerializeField] private TextMeshProUGUI headerStatus; // "Online"
    [SerializeField] private Button backButton;

    [Header("메시지")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform messageContent;     // 말풍선 부모 (Vertical Layout Group)
    [SerializeField] private GameObject npcBubblePrefab;   // 회색 말풍선 (자식에 TMP 텍스트)
    [SerializeField] private GameObject playerBubblePrefab;// 초록 말풍선 (자식에 TMP 텍스트)
    [SerializeField] private GameObject typingIndicator;   // "입력 중" 표시 (선택)

    [Header("답변")]
    [SerializeField] private GameObject choiceArea;
    [SerializeField] private Button[] choiceButtons = new Button[2];
    [SerializeField] private TextMeshProUGUI[] choiceTexts = new TextMeshProUGUI[2];

    private RoomRuntime room;
    private System.Action onBack;
    private Coroutine playing;

    // 목록에서 방을 선택하면 호출
    public void Open(string roomId, System.Action onBack)
    {
        this.onBack = onBack;
        room = PhoneChatManager.Instance != null ? PhoneChatManager.Instance.GetRoom(roomId) : null;
        if (room == null) return;

        if (headerName != null) headerName.text = room.data.displayName;
        if (headerStatus != null) headerStatus.text = "Online";
        if (typingIndicator != null) typingIndicator.SetActive(false);

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackButton);
        }

        ClearMessages();
        HideChoices();

        // 이전(또는 세이브된) 대화 기록 복원
        foreach (var line in room.history)
            SpawnBubble(line.sender, line.text);
        ScrollToBottom();

        if (room.completed) return;     // 이미 끝난 대화
        if (room.awaitingChoice)        // 답변 대기 상태에서 닫았다가 다시 연 경우
        {
            ShowChoices();
            return;
        }

        // 현재 노드부터 재생 (세이브는 노드 시작 전 history만 담고 있으므로 중복 없음)
        playing = StartCoroutine(PlayNode(room.currentNodeId));
    }

    public void OnBackButton()
    {
        if (playing != null) { StopCoroutine(playing); playing = null; }
        if (typingIndicator != null) typingIndicator.SetActive(false);
        onBack?.Invoke();
    }

    private IEnumerator PlayNode(string nodeId)
    {
        var node = room.data.GetNode(nodeId);
        if (node == null) { Complete(); yield break; }

        // [체크포인트] 노드 시작: 이 시점 history에는 아직 이 노드 메시지가 없음
        room.currentNodeId = nodeId;
        room.awaitingChoice = false;
        Save();

        foreach (var text in node.messages)
        {
            if (messageWaitTime > 0f) yield return new WaitForSecondsRealtime(messageWaitTime);

            if (typingIndicator != null) typingIndicator.SetActive(true);
            if (typingTime > 0f) yield return new WaitForSecondsRealtime(typingTime);
            if (typingIndicator != null) typingIndicator.SetActive(false);

            AddLine(ChatSender.Npc, text); // history에만 추가(저장은 다음 체크포인트에서)
        }

        if (node.IsEnd)
        {
            Complete(); // [체크포인트] 완료 시 저장
        }
        else
        {
            // [체크포인트] 답변 대기: 이제 history에 이 노드 메시지가 모두 포함됨
            room.awaitingChoice = true;
            Save();
            ShowChoices();
        }
        playing = null;
    }

    private void ShowChoices()
    {
        var node = room.data.GetNode(room.currentNodeId);
        if (node == null || node.choices.Count == 0) { HideChoices(); return; }

        if (choiceArea != null) choiceArea.SetActive(true);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < node.choices.Count)
            {
                int idx = i;
                choiceButtons[i].gameObject.SetActive(true);
                if (choiceTexts[i] != null) choiceTexts[i].text = node.choices[i].text;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => OnChoose(idx));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnChoose(int index)
    {
        var node = room.data.GetNode(room.currentNodeId);
        if (node == null || index >= node.choices.Count) return;

        var choice = node.choices[index];
        AddLine(ChatSender.Player, choice.text); // 내가 보낸 메시지(초록)
        room.awaitingChoice = false;
        HideChoices();

        var next = room.data.GetNode(choice.nextNodeId);
        if (next == null) Complete();  // 빈 값 / END / 없는 노드 → 종료(저장 포함)
        else playing = StartCoroutine(PlayNode(choice.nextNodeId)); // 다음 노드 시작 시 저장
    }

    private void Complete()
    {
        room.awaitingChoice = false;
        HideChoices();
        PhoneChatManager.Instance.MarkCompleted(room.data.roomId); // 배지 제거 + 저장
    }

    // ── 유틸 ──

    private void Save()
    {
        if (PhoneChatManager.Instance != null) PhoneChatManager.Instance.SaveProgress();
    }

    private void AddLine(ChatSender sender, string text)
    {
        room.history.Add(new DisplayedLine { sender = sender, text = text });
        SpawnBubble(sender, text);
    }

    private void SpawnBubble(ChatSender sender, string text)
    {
        var prefab = sender == ChatSender.Npc ? npcBubblePrefab : playerBubblePrefab;
        if (prefab == null) return;

        var go = Instantiate(prefab, messageContent);

        var bubble = go.GetComponent<ChatBubble>();
        if (bubble != null)
        {
            bubble.SetText(text);   // 글자 길이에 맞춰 박스 크기 자동 계산
        }
        else
        {
            var t = go.GetComponentInChildren<TextMeshProUGUI>();
            if (t != null) t.text = text;
        }

        ScrollToBottom();
    }
    private void ClearMessages()
    {
        if (messageContent == null) return;
        for (int i = messageContent.childCount - 1; i >= 0; i--)
            Destroy(messageContent.GetChild(i).gameObject);
    }

    private void HideChoices()
    {
        if (choiceArea != null) choiceArea.SetActive(false);
    }

    private void ScrollToBottom()
    {
        if (scrollRect == null) return;
        Canvas.ForceUpdateCanvases();
        if (messageContent is RectTransform rt)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        scrollRect.verticalNormalizedPosition = 0f;
    }
}