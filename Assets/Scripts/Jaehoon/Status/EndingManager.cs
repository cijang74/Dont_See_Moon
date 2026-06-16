using System.Collections; // 💡 코루틴(IEnumerator)을 사용하기 위해 반드시 추가해야 합니다.
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingManager : Singleton<EndingManager>
{
    public void StartNormalEnding()
    {
        // 8일차 노말 엔딩 시작
        StartCoroutine(EndingRoutine(8));
    }

    public void StartBadEnding()
    {
        // 9일차 배드 엔딩 시작
        StartCoroutine(EndingRoutine(9));
    }

    public void StartAllKillEnding()
    {
        // 10일차 몰살 엔딩 시작
        StartCoroutine(EndingRoutine(10));
    }

    // 💡 [핵심] 씬 이동 -> 대화 끝날 때까지 대기 -> 타이틀 씬 이동을 순차적으로 처리하는 코루틴
    private IEnumerator EndingRoutine(int day)
    {
        // 1. 엔딩 씬으로 로드
        SceneManager.LoadScene("Scene_Ending");

        // (선택) 씬 로딩 직후 오브젝트들이 초기화될 1프레임 여유를 주면 버그 예방에 좋습니다.
        yield return null;

        // 2. 대화 매니저의 코루틴이 완전히 끝날 때까지(대화가 다 끝날 때까지) 대기합니다.
        yield return StartCoroutine(DialogueManager.Instance.StartDialogue(day, InteractionObjectType.Ending, false));

        // 3. 대화 코루틴이 종료되면(플레이어가 마지막 클릭을 마치면) 시작 씬으로 돌아갑니다.
        SceneManager.LoadScene("Scene_Start");
    }
}