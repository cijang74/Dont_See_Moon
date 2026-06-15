using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class ChatBubble : MonoBehaviour
{
    public enum Align { Left, Right }

    [SerializeField] private RectTransform balloonRect; // 말풍선 이미지(자식)
    [SerializeField] private TMP_Text messageText;      // 말풍선 안 TMP
    [SerializeField] private Align align = Align.Left;   // NPC=Left, Player=Right

    [Header("크기 설정")]
    [SerializeField] private float maxWidth = 400f;
    [SerializeField] private float paddingLeft = 40f;
    [SerializeField] private float paddingRight = 25f;
    [SerializeField] private float paddingTop = 18f;
    [SerializeField] private float paddingBottom = 18f;

    private RectTransform rowRect;

    private void Awake() => rowRect = (RectTransform)transform;

    public void SetText(string message)
    {
        if (rowRect == null) rowRect = (RectTransform)transform;

        messageText.text = message;

        float padX = paddingLeft + paddingRight;
        float padY = paddingTop + paddingBottom;

        // 글자 크기 측정
        float oneLineWidth = messageText.GetPreferredValues(message, Mathf.Infinity, Mathf.Infinity).x;
        float textWidth = Mathf.Min(oneLineWidth, maxWidth - padX);
        float textHeight = messageText.GetPreferredValues(message, textWidth, Mathf.Infinity).y;

        // 글자 영역 (말풍선 왼쪽 위 기준)
        var tr = messageText.rectTransform;
        tr.anchorMin = tr.anchorMax = tr.pivot = new Vector2(0f, 1f);
        tr.sizeDelta = new Vector2(textWidth, textHeight);
        tr.anchoredPosition = new Vector2(paddingLeft, -paddingTop);

        // 말풍선 박스 크기
        float bubbleW = textWidth + padX;
        float bubbleH = textHeight + padY;
        balloonRect.sizeDelta = new Vector2(bubbleW, bubbleH);

        // 부모(Content) 폭 기준으로 좌/우 배치 — 행 폭에 의존하지 않음
        float contentWidth = (rowRect.parent is RectTransform p) ? p.rect.width : 0f;

        balloonRect.anchorMin = balloonRect.anchorMax = balloonRect.pivot = new Vector2(0f, 1f);
        float bx = (align == Align.Right) ? Mathf.Max(0f, contentWidth - bubbleW) : 0f;
        balloonRect.anchoredPosition = new Vector2(bx, 0f);

        // 행 높이만 지정 (가로폭은 VLG/Content가 처리)
        var s = rowRect.sizeDelta;
        s.y = bubbleH;
        rowRect.sizeDelta = s;
    }
}