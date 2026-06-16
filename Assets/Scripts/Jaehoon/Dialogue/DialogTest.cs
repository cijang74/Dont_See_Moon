// using System.Collections;
// using UnityEngine;
// using UnityEngine.SceneManagement;

// public enum InteractionObjectType
// {
// 	Player,
//     James,
//     Nicholas,
//     Ella,
//     Sophia
// };

// public class DialogTest : Singleton<DialogTest>
// {
// 	[SerializeField] ScriptDataLoader scriptDataLoader;
// 	[SerializeField] DialogueSystem dialogSystem; // 대화 시스템
// 	[SerializeField] float waitToLoadTime = 1f;

// 	// 외부에서 해당 메서드 호출되면 대화 시작
// 	public IEnumerator StartDialogue(int day, InteractionObjectType interactionObjectType)
// 	{
// 		scriptDataLoader.LoadScriptData(day, interactionObjectType);
// 		yield return new WaitUntil(()=>dialogSystem.UpdateDialog());

// 		// TODO: 대화 끝나면 UI 비활성화
// 	} 
// }