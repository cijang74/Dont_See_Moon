using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ScriptDataLoader : MonoBehaviour
{
    // 로드한 전체 스크립트 SO 원본을 보관
    private ScriptDataSO currentScriptSO;
    [HideInInspector] public string title;
    [HideInInspector] public int episodeNum;
    [SerializeField] public bool isTutorial = false;

    // List로 저장하고있던 SO를 Dictionary로 변환하여 사용
    public Dictionary<string, ScriptLine> dialogueDict = new Dictionary<string, ScriptLine>();

    void Awake()
    {
        if(isTutorial)
        {
            LoadTutorialScripts();
        }

        else
        {
            LoadCurrentScripts();
        }
    }

    void LoadCurrentScripts()
    {
        if(CurrentSelectDataManager.Instance == null)
        {
            Debug.LogError("CurrentSelectDataManager.Instance 접근 불가");
            return;
        }

        // 싱글톤에서 선택한 스토리 번호 가져오기
        int targetScriptNum = CurrentSelectDataManager.Instance.currentSelectStoryNum;
        int targetScriptChapter = (targetScriptNum - 1) / 10 + 1;
        int targetScruptEpisode = (targetScriptNum - 1) % 10 + 1;

        // 2. Resources 경로 설정 및 SO 데이터 로드
        // 파일명이 "Chapter1-1_Script.asset" 형태라고 가정
        // 주의: CurrentSelectDataManager에서 int형을 쓰므로, 여기서는 "Chapter1-{targetScriptNum}" 형태로 포맷팅합니다.
        string fileName = $"Chapter{targetScriptChapter}-{targetScruptEpisode}_Script"; 
        string resourcePath = $"BakedData/ScriptData/{fileName}"; 

        // 지정된 경로에서 ScriptDataSO 에셋을 동적으로 불러옴
        currentScriptSO = Resources.Load<ScriptDataSO>(resourcePath);

        if (currentScriptSO == null)
        {
            Debug.LogError($"SO 파일을 찾을 수 없습니다! 경로를 확인해주세요: Resources/{resourcePath}");
            SceneManager.LoadScene("Scene_ChapterSelect");
            return;
        }

        // 3. 시간복잡도를 줄이기 위해 List를 Dictionary로 변환
        dialogueDict.Clear(); // 다른 스토리를 로드할 수도 있으니 딕셔너리 초기화

        title = currentScriptSO.title;
        episodeNum = targetScruptEpisode;

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

    void LoadTutorialScripts()
    {
        // 2. Resources 경로 설정 및 SO 데이터 로드
        // 파일명이 "Chapter1-1_Script.asset" 형태라고 가정
        // 주의: CurrentSelectDataManager에서 int형을 쓰므로, 여기서는 "Chapter1-{targetScriptNum}" 형태로 포맷팅합니다.
        string fileName = $"Tutorial_Script"; 
        string resourcePath = $"BakedData/ScriptData/{fileName}"; 

        // 지정된 경로에서 ScriptDataSO 에셋을 동적으로 불러옴
        currentScriptSO = Resources.Load<ScriptDataSO>(resourcePath);

        if (currentScriptSO == null)
        {
            Debug.LogError($"SO 파일을 찾을 수 없습니다! 경로를 확인해주세요: Resources/{resourcePath}");
            SceneManager.LoadScene("Scene_Main");
            return;
        }

        // 3. 시간복잡도를 줄이기 위해 List를 Dictionary로 변환
        dialogueDict.Clear(); // 다른 스토리를 로드할 수도 있으니 딕셔너리 초기화

        title = currentScriptSO.title;
        episodeNum = 0;

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

    // 외부(대화 UI 매니저 등)에서 특정 ID의 대사 데이터가 필요할 때 호출할 메서드
    // public ScriptLine GetScriptLine(string targetID)
    // {
    //     if (dialogueDict.TryGetValue(targetID, out ScriptLine line))
    //     {
    //         return line; // 딕셔너리를 통해 O(1)로 즉시 반환
    //     }
        
    //     Debug.LogWarning($"해당 ID({targetID})의 대사 데이터를 찾을 수 없습니다.");
    //     return null; // 없는 ID를 호출하면 null 반환
    // }
}