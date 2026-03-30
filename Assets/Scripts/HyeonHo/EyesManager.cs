using System;
using System.Collections;
using Seagull.Interior_01.Utility;
using UnityEngine;

public class EyesManager : MonoBehaviour
{
    private FadeSystem fadeScript;

    private float openEyesAlpha = 0f;

    private float closeEyesAlpha = 1f;

    private float fadingTime = 0.5f;

    //눈을 뜨거나 감는 중인지
    private bool isFading = false;

    //눈을 뜨고 있는지
    private bool isEyesOpening = true;

    //눈을 감고 있는지
    private bool isEyesClosing = false;

    void Start()
    {
        fadeScript = GameObject.Find("EventSystem").GetComponent<FadeSystem>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && !isFading && isEyesOpening)
        {
            Debug.Log("눈을 감는 중입니다.");
            StartCoroutine(CloseEyes());
        }

        if(Input.GetKeyUp(KeyCode.Space) && !isEyesOpening)
        {
            Debug.Log("눈을 뜨는 중입니다.");
            StartCoroutine(OpenEyes());
        }
    }

    private IEnumerator CloseEyes()
    {
        isEyesOpening = false;
        isFading = true;
        isEyesClosing = true;

        StartCoroutine(fadeScript.FadeIn(openEyesAlpha, closeEyesAlpha, fadingTime));
        
        yield return new WaitForSeconds(fadingTime);

        isFading = false;
    }

    private IEnumerator OpenEyes()
    {
        isEyesOpening = true;

        if(isFading)
        {
            //isFading이 false가 될 때까지 대기
            yield return new WaitUntil(() => !isFading);
        }

        isEyesClosing = false;
        isFading = true;
        StartCoroutine(fadeScript.FadeIn(closeEyesAlpha, openEyesAlpha, fadingTime));
        
        yield return new WaitForSeconds(fadingTime);

        isFading = false;
    }
}
