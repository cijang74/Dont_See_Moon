using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BedEvent:MonoBehaviour
{
    [SerializeField] private Image fadeImage;

    //처음 투명도
    private float startAlpha = 0f;

    //처음 투명도
    private float endAlpha = 0.99f;

    //암전 되는 시간
    private float fadeTime = 1.6f;

    //암전 되어있는 시간
    private float sleepTime = 3f;

    // 카메라가 암전 중인지 확인
    private bool isFading = false;

    //날짜
    private int day = 1;
    
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
            StartCoroutine(Fade(startAlpha, endAlpha, fadeTime, sleepTime));
            Debug.Log("잠을 자는 중입니다.");
            day++;
        }
        else
        {
            Debug.LogWarning("이미 잠을 자는 중입니다.");
        }
    }

    private IEnumerator Fade(float start, float end, float fade, float sleep)
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

        //검어진 상태로 기다리는 시간
        yield return new WaitForSeconds(sleep);

        //화면 다시 밝아지는 함수
        elapsed = 0f;
        while(elapsed < fade)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fade;

            color.a = Mathf.Lerp(end, start, t);
            fadeImage.color = color;

            yield return null;
        }

        //밝기를 원상태로 복구(정확하지 않은 수치일 수 있어서)
        color.a = start;
        fadeImage.color = color;

        //전환 완료
        isFading = false;
    }

    public bool GetIsFading()
    {
        return isFading;
    }
    public int GetDay()
    {
        return day;
    }
}

