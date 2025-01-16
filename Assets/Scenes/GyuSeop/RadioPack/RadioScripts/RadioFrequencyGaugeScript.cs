using UnityEngine;
using TMPro;
using System.Linq;

public class RadioFrequencyGaugeScript : MonoBehaviour
{
    public TMP_Text frequencyDisplay; // 주파수 텍스트

    public Transform frequencyNeedle; // 바늘 Transform
    public Transform frequencyGaugeBackground; // 배경 Transform

    private float minX; // 바늘 이동 범위의 최소 X
    private float maxX; // 바늘 이동 범위의 최대 X

    void Start()
    {
        CalculateNeedleBounds();
        minX = -0.5f;
        maxX = 0.5f; //CalculateNeedleBounds() 개같이 안돼서 그냥 수치 꼬라박음, 나중에 게이지 크기 바뀌면 바꿔야 할수도, 근데 느낌상 안바꿔도 될듯
        RadioManagerScript.Instance.radioFrequency=RadioManagerScript.Instance.minFrequency;
        frequencyDisplay.text = $"{RadioManagerScript.Instance.radioFrequency:F2} MHz"; //주파수 표시창 초기화화
        UpdateNeedlePosition(); // 초기 바늘 위치 설정
        Debug.Log($"minX: {minX}, maxX: {maxX}");
    }

    void Update()
    {
        if(RadioManagerScript.Instance.isDragging == true) //돌아가면 텍스트 갱신 시작
        {
            ChangeFrequencyText();
        }
    }

    void ChangeFrequencyText()
    {
        RadioManagerScript.Instance.radioFrequency = Mathf.Clamp(RadioManagerScript.Instance.radioFrequency, RadioManagerScript.Instance.minFrequency, RadioManagerScript.Instance.maxFrequency);
        UpdateDisplay();
        UpdateNeedlePosition(); // 바늘 위치 업데이트
    }

    private void UpdateDisplay()
    {
        frequencyDisplay.text = $"{RadioManagerScript.Instance.radioFrequency:F2} MHz";
    }

    private void UpdateNeedlePosition()
    {
        // 현재 주파수를 0~1 범위로 정규화
        float normalizedFrequency = (RadioManagerScript.Instance.radioFrequency - RadioManagerScript.Instance.minFrequency) /
                                    (RadioManagerScript.Instance.maxFrequency - RadioManagerScript.Instance.minFrequency);

        // 바늘의 X 위치 계산
        float needleX = Mathf.Lerp(minX, maxX, normalizedFrequency);

        // 바늘의 로컬 위치 업데이트 (y, z는 고정)
        frequencyNeedle.localPosition = new Vector3(needleX, frequencyNeedle.localPosition.y, frequencyNeedle.localPosition.z);
    }
/*
    private void CalculateNeedleBounds()
    {
        // 배경의 크기 기준으로 이동 범위 설정 (로컬 좌표 사용)
        SpriteRenderer backgroundRenderer = frequencyGaugeBackground.GetComponent<SpriteRenderer>();
        if (backgroundRenderer != null)
        {
            Bounds bounds = backgroundRenderer.bounds;

            // 배경의 최소/최대 X 좌표를 로컬 좌표로 변환
            Vector3 localMin = frequencyGaugeBackground.InverseTransformPoint(bounds.min);
            Vector3 localMax = frequencyGaugeBackground.InverseTransformPoint(bounds.max);

            // 이동 범위 설정
            minX = localMin.x;
            maxX = localMax.x;
        }
        else
        {
            Debug.LogError("frequencyGaugeBackground에 SpriteRenderer가 없습니다!");
        }
    }*/

    private void CalculateNeedleBounds()
    {
        // 배경 스프라이트의 크기 기준으로 이동 범위 설정 (로컬 좌표 사용)
        SpriteRenderer backgroundRenderer = frequencyGaugeBackground.GetComponent<SpriteRenderer>();
        if (backgroundRenderer != null)
        {
            Sprite sprite = backgroundRenderer.sprite;

            if (sprite != null)
            {
                // 스프라이트의 로컬 정점 가져오기
                Vector2[] vertices = sprite.vertices;
                Transform backgroundTransform = frequencyGaugeBackground.transform;

                // 로컬 정점을 월드 좌표로 변환
                Vector3[] worldVertices = new Vector3[vertices.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 localVertex = new Vector3(vertices[i].x, vertices[i].y, 0); // Vector2 → Vector3 변환
                    worldVertices[i] = backgroundTransform.TransformPoint(localVertex); // 로컬 → 월드 변환
                }

                // 월드 정점 중 X축의 최소/최대 값 계산
                float worldMinX = worldVertices.Min(v => v.x);
                float worldMaxX = worldVertices.Max(v => v.x);

                // 월드 좌표를 로컬 좌표로 변환
                Vector3 localMin = backgroundTransform.InverseTransformPoint(new Vector3(worldMinX, 0, 0));
                Vector3 localMax = backgroundTransform.InverseTransformPoint(new Vector3(worldMaxX, 0, 0));

                // 이동 범위 설정
                minX = localMin.x;
                maxX = localMax.x;
            }
            else
            {
                Debug.LogError("SpriteRenderer에 스프라이트가 설정되어 있지 않습니다!");
            }
        }
        else
        {
            Debug.LogError("frequencyGaugeBackground에 SpriteRenderer가 없습니다!");
        }
    }

}