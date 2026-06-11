using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ArrowBlink : MonoBehaviour
{
	[SerializeField]
	private float fadeTime;    // 알파값이 0에서 1(또는 1에서 0)로 변하는 데 걸리는 목표 시간(초 단위)
	private Image fadeImage;   // 페이드 효과를 적용할 대상 UI Image 컴포넌트

	private void Awake()
	{
		// 스크립트가 부착된 게임 오브젝트에서 Image 컴포넌트를 가져옵니다.
		fadeImage = GetComponent<Image>();
	}

	private void OnEnable()
	{
		// 게임 오브젝트가 활성화(SetActive(true))될 때마다 페이드 코루틴을 시작합니다.
		// 문자열로 코루틴을 실행하면 이후 StopCoroutine을 문자열로 호출해 직관적으로 멈출 수 있습니다.
		StartCoroutine("FadeInOut");
	}

	private void OnDisable()
	{
		// 게임 오브젝트가 비활성화되면 실행 중이던 코루틴을 안전하게 정지시킵니다.
		StopCoroutine("FadeInOut");
	}

	private IEnumerator FadeInOut()
	{
		// while (true)를 통해 오브젝트가 켜져 있는 동안 무한 반복합니다.
		while ( true )
		{
			// StartCoroutine을 yield return으로 대기하여, 페이드 아웃이 완전히 끝날 때까지 기다립니다.
			yield return StartCoroutine(Fade(1, 0));	// 불투명(1) -> 투명(0)으로 서서히 변경

			// 페이드 아웃이 끝나면 바로 페이드 인을 시작합니다.
			yield return StartCoroutine(Fade(0, 1));	// 투명(0) -> 불투명(1)으로 서서히 변경
		}
	}

	// 실제 투명도(Alpha) 값을 시간에 따라 부드럽게 변경하는 핵심 코루틴
	private IEnumerator Fade(float start, float end)
	{
		float current = 0;   // 현재 경과 시간
		float percent = 0;   // 진행률 (0.0 ~ 1.0)

		// 진행률이 1(100%)이 될 때까지 매 프레임 반복합니다.
		while ( percent < 1 )
		{
			current += Time.deltaTime;     // 이전 프레임부터 지금까지 걸린 시간을 누적
			percent = current / fadeTime;  // 전체 목표 시간 중 어느 정도 왔는지 비율 계산

			Color color = fadeImage.color;
			// Mathf.Lerp를 사용해 start와 end 사이의 값을 percent 비율에 맞춰 부드럽게 보간합니다.
			color.a = Mathf.Lerp(start, end, percent);
			fadeImage.color = color;

			// 다음 프레임까지 렌더링을 대기합니다.
			yield return null;
		}
	}
}