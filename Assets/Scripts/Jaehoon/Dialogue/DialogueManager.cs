using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractionObjectType
{
    None = 0,
	Player,
    James,
    Nicholas,
    Ella,
    Sophia,
    Vote,
    Day1_BloodKnife,
    Day1_Pee
};

public class DialogueManager : Singleton<DialogueManager>
{
	[SerializeField] DialogueSystem dialogSystem; // 대화 시스템
	[SerializeField] GameObject dialoguePanel;
    [SerializeField] GameObject dialogueCanvas;

	[SerializeField] float waitToLoadTime = 1f;

    // 로드한 전체 스크립트 SO 원본을 보관
    private ScriptDataSO currentScriptSO;

    // List로 저장하고있던 SO를 Dictionary로 변환하여 사용
    public Dictionary<string, ScriptLine> dialogueDict = new Dictionary<string, ScriptLine>();

    // 테스트용
    void Start()
    {
        EvidenceManager.Instance.AcquireEvidence(InteractionObjectType.Day1_BloodKnife);
        StartCoroutine(StartDialogue(1, InteractionObjectType.James, false));
    }

    // 외부에서 해당 메서드 호출되면 대화 시작
    public IEnumerator StartDialogue(int day, InteractionObjectType interactionObjectType, bool isInfected)
	{
		LoadScriptData(day, interactionObjectType, isInfected);
        dialogSystem.ResetDialogueState();
        dialogueCanvas.SetActive(true);
        dialoguePanel.SetActive(true);
		yield return new WaitUntil(()=>dialogSystem.UpdateDialog());

		// TODO: 대화 끝나면 UI 비활성화
	} 

    public void LoadScriptData(int day, InteractionObjectType interactionObjectType, bool isInfected)
    {
        string targetDay = day.ToString();
        string targetType = interactionObjectType.ToString();
        string targetInfected = isInfected ? "Infected" : "Normal";

        string fileName = $"Day{targetDay}-{targetType}-{targetInfected}_Script"; 
        string resourcePath = $"BakedData/ScriptData/{fileName}"; 

        // 지정된 경로에서 ScriptDataSO 에셋을 동적으로 불러옴
        currentScriptSO = Resources.Load<ScriptDataSO>(resourcePath);

        if (currentScriptSO == null)
        {
            Debug.LogError($"SO 파일을 찾을 수 없습니다! 경로를 확인해주세요: Resources/{resourcePath}");
            return;
        }

        // 3. 시간복잡도를 줄이기 위해 List를 Dictionary로 변환
        dialogueDict.Clear(); // 다른 스토리를 로드할 수도 있으니 딕셔너리 초기화

        // dialogueLines는 기존 스크립트 라인들을 담고있던 리스트
        foreach (ScriptLine line in currentScriptSO.dialogueLines)
        {
            // ID가 중복으로 들어가는 것을 방지하기 위한 안전장치
            if (!dialogueDict.ContainsKey(line.ID))
            {
                dialogueDict.Add(line.ID, line);
            }

            else
            {
                Debug.LogWarning($"[ScriptDataLoader] 중복된 대사 ID가 발견되었습니다: {line.ID}. 엑셀 데이터를 확인해주세요.");
            }
        }

        Debug.Log($"[성공] {fileName} 로드 완료! 총 {dialogueDict.Count}개의 대사가 딕셔너리로 변환되었습니다.");
    }
}
