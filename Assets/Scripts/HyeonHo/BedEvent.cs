using System;
using System.Collections;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class BedEvent:MonoBehaviour
{
    private FadeSystem fadeScript;

    private HungerSystem hungerScript;

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

    //자고 일어나서 다시 자는데까지 걸린 시간
    private float sleepToSleepTime = 0f;
    
    void Awake()
    {
        fadeScript = GameObject.Find("EventSystem").GetComponent<FadeSystem>();
        hungerScript = GameObject.Find("EventSystem").GetComponent<HungerSystem>();
    }

    void Update()
    {

    }

    void OnMouseUp()
    {
        // 객체가 클릭되었을 때 화면 암전
        if (!isFading)
        {
            isFading = true;
            CancelInvoke("Timer");
            StartCoroutine(Fade(startAlpha, endAlpha, fadeTime, sleepTime));
            Debug.Log("잠을 자는 중입니다.");
        }
        else
        {
            Debug.LogWarning("이미 잠을 자는 중입니다.");
        }
    }

    private IEnumerator Fade(float start, float end, float fade, float sleep)
    {
        StartCoroutine(fadeScript.FadeIn(start, end, fade));

        //검어진 상태로 기다리는 시간
        yield return new WaitForSeconds(sleep);

        day++;
        Debug.Log("Day : " + day);

        StartCoroutine(fadeScript.FadeIn(end, start, fade));

        //전환 완료
        isFading = false;
        
        InitForNewDay();
    }

    private void InitForNewDay()
    {
        hungerScript.DecreaseHungerAfterSleep(sleepToSleepTime);
        sleepToSleepTime = 0f;
        InvokeRepeating("Timer", 0f, 1f);
    }

    private void Timer()
    {
        sleepToSleepTime += 1f;
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

