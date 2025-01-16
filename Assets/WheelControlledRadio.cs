using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RadioManager : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform needle; // 바늘 RectTransform
    public RectTransform gaugeBackground; // 게이지 배경 RectTransform
    public TMP_Text frequencyDisplay; // 주파수 텍스트

    [Header("Frequency Settings")]
    public float minFrequency = 88.0f;
    public float maxFrequency = 108.0f;

    private float minX;
    private float maxX;
    private float currentFrequency;
    private float needleX;

    void Start()
    {
        // 바늘 이동 범위 계산
        minX = gaugeBackground.rect.xMin;
        maxX = gaugeBackground.rect.xMax;

        WheelController wheelController = FindObjectOfType<WheelController>();
        wheelController.OnWheelRotated += UpdateNeedle;

        // 초기 주파수 설정
        currentFrequency = minFrequency;
        UpdateDisplay();
    }

    public void UpdateNeedle(float deltaAngle)
    {
        // 바늘 이동 계산
        float movementRange = maxX - minX;
        needleX += deltaAngle / 360f * movementRange;
        needleX = Mathf.Clamp(needleX, minX, maxX);

        // 바늘 위치 업데이트
        needle.localPosition = new Vector3(needleX, needle.localPosition.y, needle.localPosition.z);

        // 주파수 업데이트
        float normalizedPosition = (needleX - minX) / movementRange;
        currentFrequency = Mathf.Lerp(minFrequency, maxFrequency, normalizedPosition);
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        frequencyDisplay.text = $"{currentFrequency:F1} MHz";
    }
}
