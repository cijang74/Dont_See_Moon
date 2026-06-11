using UnityEngine;

[RequireComponent(typeof(Light))]
public class HorrorLightFlicker : MonoBehaviour
{
    [Header("불규칙한 일렁임 (전압 불안정)")]
    [Tooltip("조명의 최소 밝기")]
    public float minIntensity = 14f;
    [Tooltip("조명의 최대 밝기")]
    public float maxIntensity = 18f;
    [Tooltip("일렁이는 속도")]
    public float flickerSpeed = 5f;

    [Header("고장난 전구 효과 (갑작스러운 꺼짐)")]
    public bool useStutter = false;
    [Tooltip("순간적으로 꺼질 확률 (높을수록 자주 꺼짐)")]
    public float stutterChance = 2.0f; 
    [Tooltip("꺼져 있는 유지 시간")]
    public float stutterDuration = 0.1f; 

    private Light targetLight;
    private float randomNoiseOffset;
    private float stutterTimer;

    void Start()
    {
        targetLight = GetComponent<Light>();
        
        // 씬에 여러 개의 조명이 있을 때, 모두 똑같이 깜빡이는 것을 방지하기 위한 난수
        randomNoiseOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        // 1. 순간적으로 꺼지는 효과 (Stutter)
        if (useStutter)
        {
            if (stutterTimer > 0)
            {
                // 꺼져 있는 시간 동안 타이머 감소시키고 밝기를 0(혹은 매우 낮게) 유지
                stutterTimer -= Time.deltaTime;
                targetLight.intensity = 1f; 
                return; // 아래의 노이즈 일렁임 계산은 건너뜀
            }

            // 프레임 레이트에 독립적으로 꺼짐 확률 계산
            if (Random.value < stutterChance * Time.deltaTime)
            {
                // 꺼짐 발동 시 타이머 설정
                stutterTimer = Random.Range(0.05f, stutterDuration);
                return;
            }
        }

        // 2. 부드럽지만 불규칙한 밝기 변화 (Perlin Noise)
        // Time.time에 속도를 곱해 노이즈 샘플링 위치를 이동시킴
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed + randomNoiseOffset, 0f);
        
        // 0~1 사이의 노이즈 값을 우리가 설정한 최소~최대 밝기 값으로 변환
        targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}