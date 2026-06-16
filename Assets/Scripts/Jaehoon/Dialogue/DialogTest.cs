using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogTest : MonoBehaviour
{
	[SerializeField] DialogueSystem dialogSystem01; // 첫 번째 대화 분기를 담당하는 시스템
	// [SerializeField] FoldingMenu foldingMenu;
	[SerializeField] float waitToLoadTime = 1f;

	private IEnumerator Start()
	{
		// dialogSystem01.UpdateDialog가 true를 반환할 때 까지 매 프레임 대기
		yield return new WaitUntil(()=>dialogSystem01.UpdateDialog());

		// 대화가 모두 끝나면 스토리 선택씬으로 이동
		StartCoroutine(LoadSceneRoutine());
	}

	private IEnumerator LoadSceneRoutine()
    {
        // 페이드아웃할 시간 1초 기다려 주고 씬 불러오도록 해주는 코루틴
        while(waitToLoadTime >= 0)
        {
            waitToLoadTime -= Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(waitToLoadTime);

		// 연속 진행 활성화시 다음 에피소드 진행
		if(CurrentSelectDataManager.Instance.storyContinuousProgress)
		{
			CurrentSelectDataManager.Instance.currentSelectStoryNum += 1;

			Debug.Log($"{CurrentSelectDataManager.Instance.currentSelectStoryNum}에피소드로 이동");

			// 10 이상이면 챕터 선택씬으로 이동
			if(CurrentSelectDataManager.Instance.currentSelectStoryNum >= 10)
			{
				SceneManager.LoadScene("Scene_ChapterSelect");
			}

			else
			{
				SceneManager.LoadScene("Scene_Dialogue");
			}
		}
    }
}