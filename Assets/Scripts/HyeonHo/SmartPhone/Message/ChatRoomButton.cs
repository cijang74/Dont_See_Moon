using UnityEngine;
using UnityEngine.UI; // Button
using TMPro;          // TextMeshProUGUI

// 채팅방 목록의 버튼 하나 (이름 + 빨간 배지)
public class ChatRoomButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText; // "엄마: 온라인"
    [SerializeField] private GameObject unreadBadge;   // 오른쪽 위 빨간 동그라미 + 느낌표
    [SerializeField] private Button button;
    [SerializeField] private string statusSuffix = ": 온라인";

    private RoomRuntime room;

    public void Setup(RoomRuntime room, System.Action<string> onClick)
    {
        this.room = room;

        if (nameText != null) nameText.text = room.data.displayName + statusSuffix;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke(room.data.roomId));
        }

        RefreshBadge();
    }

    // 배지 표시 갱신
    public void RefreshBadge()
    {
        if (unreadBadge != null && room != null)
            unreadBadge.SetActive(room.hasUnread);
    }
}