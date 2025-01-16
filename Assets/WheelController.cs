using UnityEngine;
using UnityEngine.EventSystems;

public class WheelController : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public delegate void WheelRotationHandler(float deltaAngle);
    public event WheelRotationHandler OnWheelRotated;

    private float currentAngle;

    public void OnPointerDown(PointerEventData eventData)
    {
        // 클릭 시 초기화
        Vector2 localCursor;
        RectTransform rect = GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out localCursor))
        {
            currentAngle = Mathf.Atan2(localCursor.y, localCursor.x) * Mathf.Rad2Deg;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localCursor;
        RectTransform rect = GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out localCursor))
        {
            float newAngle = Mathf.Atan2(localCursor.y, localCursor.x) * Mathf.Rad2Deg;
            float deltaAngle = Mathf.DeltaAngle(currentAngle, newAngle);
            currentAngle = newAngle;

            // 이벤트로 회전값 전달
            OnWheelRotated?.Invoke(deltaAngle);

            // 휠 시각적으로 회전
            transform.localEulerAngles += new Vector3(0, 0, -deltaAngle);
        }
    }
}
