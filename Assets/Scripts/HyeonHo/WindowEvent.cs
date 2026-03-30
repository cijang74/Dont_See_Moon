using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WindowEvent : MonoBehaviour
{
    [SerializeField] private Image fadeImage;

    //처음 투명도
    private float startAlpha = 0f;

    //처음 투명도
    private float endAlpha = 0.99f;

    //암전 되는 시간
    private float fadeTime = 1.6f;

    // 카메라가 암전 중인지 확인
    private bool isFading = false;
    
    void Start()
    {
        
    }

    void Update()
    {

    }

    void OnMouseDown()
    {
        // 객체가 클릭되었을 때 화면 암전
        if (!isFading)
        {
            isFading = true;
            StartCoroutine(Fade(startAlpha, endAlpha, fadeTime));
            Debug.Log("잠을 자는 중입니다.");
        }
        else
        {
            Debug.LogWarning("이미 잠을 자는 중입니다.");
        }
    }

    private IEnumerator Fade(float start, float end, float fade)
    {
        //화면 검어지는 함수
        float elapsed = 0f;
        Color color = fadeImage.color;

        while(elapsed < fade)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fade;

            color.a = Mathf.Lerp(start, end, t);
            fadeImage.color = color;

            yield return null;
        }

        //밝기를 원하는 수치로 정확하게 설정(정확하지 않은 수치일 수 있어서)
        color.a = end;
        fadeImage.color = color;

        //전환 완료
        isFading = false;
    }

    public bool GetIsFading()
    {
        return isFading;
    }
}


