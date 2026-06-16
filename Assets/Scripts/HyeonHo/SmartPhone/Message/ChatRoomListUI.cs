using System.Collections.Generic;
using UnityEngine;

// 채팅방 목록 화면: 방 버튼들을 생성하고, 방을 선택하면 대화 화면으로 전환
public class ChatRoomListUI : MonoBehaviour
{
    [Header("목록")]
    [SerializeField] private Transform listContent;          // 버튼 부모 (Vertical Layout Group 권장)
    [SerializeField] private ChatRoomButton roomButtonPrefab; // 방 버튼 프리팹

    [Header("화면 전환")]
    [SerializeField] private GameObject listPanel; // 목록 패널
    [SerializeField] private GameObject viewPanel; // 대화 패널
    [SerializeField] private ChatRoomView roomView; // 대화 화면 스크립트

    private readonly List<ChatRoomButton> spawned = new List<ChatRoomButton>();
    private bool built;

    private void Start()
    {
        Build();
    }

    private void OnEnable()
    {
        if (PhoneChatManager.Instance != null)
            PhoneChatManager.Instance.OnRoomsChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (PhoneChatManager.Instance != null)
            PhoneChatManager.Instance.OnRoomsChanged -= Refresh;
    }

    private void Build()
    {
        if (built || PhoneChatManager.Instance == null) return;

        foreach (var b in spawned) if (b != null) Destroy(b.gameObject);
        spawned.Clear();

        foreach (var room in PhoneChatManager.Instance.GetRooms())
        {
            var btn = Instantiate(roomButtonPrefab, listContent);
            btn.Setup(room, OpenRoom);
            spawned.Add(btn);
        }
        built = true;
    }

    private void Refresh()
    {
        foreach (var b in spawned) if (b != null) b.RefreshBadge();
    }

    private void OpenRoom(string roomId)
    {
        if (listPanel != null) listPanel.SetActive(false);
        if (viewPanel != null) viewPanel.SetActive(true);
        roomView.Open(roomId, BackToList);
    }

    // 대화 화면의 뒤로가기에서 호출
    public void BackToList()
    {
        if (viewPanel != null) viewPanel.SetActive(false);
        if (listPanel != null) listPanel.SetActive(true);
        Refresh();
    }
}