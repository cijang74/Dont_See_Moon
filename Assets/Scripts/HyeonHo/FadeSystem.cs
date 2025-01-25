using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeSystem : MonoBehaviour
{
    [SerializeField] private Image fadeImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public IEnumerator FadeIn(float startAlpha, float endAlpha, float fadeTime)
    {
        float elapsed = 0f;
        Color color = fadeImage.color;

        while(elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;

            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeImage.color = color;

            yield return null;
        }

        //밝기를 정확한 수치로 조정(정확하지 않은 수치일 수 있어서)
        color.a = endAlpha;
        fadeImage.color = color;
    }
}
