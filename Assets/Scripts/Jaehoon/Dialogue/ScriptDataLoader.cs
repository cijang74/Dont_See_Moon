// using UnityEngine;
// using System.Collections.Generic;

// public class ScriptDataLoader : MonoBehaviour
// {
//     // 로드한 전체 스크립트 SO 원본을 보관
//     private ScriptDataSO currentScriptSO;
//     [HideInInspector] public int episodeNum;

//     // List로 저장하고있던 SO를 Dictionary로 변환하여 사용
//     public Dictionary<string, ScriptLine> dialogueDict = new Dictionary<string, ScriptLine>();

//     public void LoadScriptData(int day, InteractionObjectType interactionObjectType)
//     {
//         string targetDay = day.ToString();
//         string targetType = interactionObjectType.ToString();

//         string fileName = $"Day{targetDay}-{targetType}_Script"; 
//         string resourcePath = $"BakedData/ScriptData/{fileName}"; 

//         // 지정된 경로에서 ScriptDataSO 에셋을 동적으로 불러옴
//         currentScriptSO = Resources.Load<ScriptDataSO>(resourcePath);

//         if (currentScriptSO == null)
//         {
//             Debug.LogError($"SO 파일을 찾을 수 없습니다! 경로를 확인해주세요: Resources/{resourcePath}");
//             return;
//         }

//         // 3. 시간복잡도를 줄이기 위해 List를 Dictionary로 변환
//         dialogueDict.Clear(); // 다른 스토리를 로드할 수도 있으니 딕셔너리 초기화

//         // dialogueLines는 기존 스크립트 라인들을 담고있던 리스트
//         foreach (ScriptLine line in currentScriptSO.dialogueLines)
//         {
//             // ID가 중복으로 들어가는 것을 방지하기 위한 안전장치
//             if (!dialogueDict.ContainsKey(line.ID))
//             {
//                 dialogueDict.Add(line.ID, line);
//             }

//             else
//             {
//                 Debug.LogWarning($"[ScriptDataLoader] 중복된 대사 ID가 발견되었습니다: {line.ID}. 엑셀 데이터를 확인해주세요.");
//             }
//         }

//         Debug.Log($"[성공] {fileName} 로드 완료! 총 {dialogueDict.Count}개의 대사가 딕셔너리로 변환되었습니다.");
//     }
// }