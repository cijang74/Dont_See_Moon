//***************************
// 파일명: CSVToSO_Dialogue.cs
// 작성자: 김재훈
// 작성일: 2026.03.23
// 내용: CSV로 작성된 데이터들을 불러와 Scriptable Object로 베이킹해주는 스크립트 (대화 스크립트는 자주 사용될 것 같아서 따로 작성함)
//***************************

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

public class CSVToSO_Dialogue : MonoBehaviour
{

    [MenuItem("Tools/Resources/DialogueScriptCSV 내 CSV 파일 -> ScriptableObject 변환")]
    public static void BakeData()
    {
        BakeScriptData(); // Unit CSV DATA -> SO

        // 변경된 에셋들을 저장하고 새로고침
        AssetDatabase.SaveAssets(); // 실제 저장
        AssetDatabase.Refresh(); // Unity Engine 새로고침
    }

    private static void BakeScriptData()
    {
        string csvResourceFolderPath = "Assets/Resources/DialogueScriptCSV";
        string saveFolderPath = "Assets/Resources/BakedData/ScriptData";

        // 폴더 존재하지 않으면 에러 발생하므로 만들어주는 방어코드
        if (!Directory.Exists(csvResourceFolderPath))
        {
            Directory.CreateDirectory(csvResourceFolderPath);
        }

        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
        }

        // Assets/Resources/ScriptCSV내부에 있는 .csv파일 모두 읽어오기
        string[] csvFiles = Directory.GetFiles(csvResourceFolderPath, "*.csv"); // csvFiles[0]에는 "Assets/Resources/CSV/Chapter1_Script.csv" 같이 저장
        
        // 파일 존재하지 않으면 종료
        if (csvFiles.Length == 0)
        {
            Debug.LogWarning($"CSV 파일이 없습니다! 경로를 확인해주세요: {csvResourceFolderPath}");
            return;
        }

        // 찾은 CSV 파일 개수만큼 반복문 돌리기
        foreach (string filePath in csvFiles)
        {
            // 경로에서 확장자를 제외한 파일 이름만 쏙 추출 ex) "Chapter1_Script"
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            
            // 파일이름 토대로 파일 읽기
            List<Dictionary<string, object>> csvData = CSV_Reader.Read_CSV($"DialogueScriptCSV/{fileName}");

            ScriptDataSO newData = ScriptableObject.CreateInstance<ScriptDataSO>();
            newData.title = GetValueStrict(csvData[0], "Title");

            // 대사 1줄씩 데이터 준비
            foreach (var data in csvData)
            {
                ScriptLine line = new ScriptLine();

                line.ID = GetValueStrict(data, "ID");
                line.speakerPosition = GetValueStrict(data, "SpeakerPosition");

                line.speakerName = GetValueStrict(data, "SpeakerName");
                line.listenerName = GetValueStrict(data, "ListenerName");
                line.dialogueText = GetValueStrict(data, "DialogueText");

                line.nextID = GetValueStrict(data, "NextID");

                string eventStr = GetValueStrict(data, "Event");
                line.events = ParsingData<DialogueEvent>(eventStr, parts =>
                {
                    DialogueEvent newEvent = new DialogueEvent();

                    if (Enum.TryParse(parts[0], out ENUM_EventType type))
                    {
                        newEvent.eventType = type;
                    }

                    // 파싱 결과 길이가 2 이상일떄만 target변수 넣기
                    if (parts.Length == 2)
                    {
                        newEvent.target = parts[1]; // ex) "Empire_Knight_Blue"
                    }

                    else
                    {
                        newEvent.target = ""; // 타겟이 없는 단일 이벤트
                    }

                    return newEvent;
                }, 1, 2);
                // line.events.Add(GetValueStrict(data, "Event1"));
                // line.events.Add(GetValueStrict(data, "Event2"));
                
                line.emotion = GetValueStrict(data, "Emotion");
                
                string effectStr = GetValueStrict(data, "Choice1_Effect");
                Choice newChoice = new Choice(GetValueStrict(data, "Choice1_Text"), GetValueStrict(data, "Choice1_NextID"),
                ParsingData<ChoiceEffect>(effectStr, parts =>
                {
                    ChoiceEffect effect = new ChoiceEffect();

                    if (Enum.TryParse(parts[0], out ENUM_EffectType type))
                    {
                        effect.effectType = type;
                        
                    } 

                    effect.effectTargetPath = parts[1];

                    int.TryParse(parts[2], out effect.effectAmount);

                    return effect;
                }, 3));
                line.choices.Add(newChoice);

                effectStr = GetValueStrict(data, "Choice2_Effect");
                newChoice = new Choice(GetValueStrict(data, "Choice2_Text"), GetValueStrict(data, "Choice2_NextID"), 
                ParsingData<ChoiceEffect>(effectStr, parts =>
                {
                    ChoiceEffect effect = new ChoiceEffect();

                    if (Enum.TryParse(parts[0], out ENUM_EffectType type))
                    {
                        effect.effectType = type;
                        
                    } 

                    effect.effectTargetPath = parts[1];

                    int.TryParse(parts[2], out effect.effectAmount);

                    return effect;
                }, 3));
                line.choices.Add(newChoice);

                effectStr = GetValueStrict(data, "Choice3_Effect");
                newChoice = new Choice(GetValueStrict(data, "Choice3_Text"), GetValueStrict(data, "Choice3_NextID"), 
                ParsingData<ChoiceEffect>(effectStr, parts =>
                {
                    ChoiceEffect effect = new ChoiceEffect();

                    if (Enum.TryParse(parts[0], out ENUM_EffectType type))
                    {
                        effect.effectType = type;
                        
                    } 

                    effect.effectTargetPath = parts[1];

                    int.TryParse(parts[2], out effect.effectAmount);

                    return effect;
                }, 3));
                line.choices.Add(newChoice);

                // 완성된 대사 1줄을 SO 내부의 리스트에 추가
                newData.dialogueLines.Add(line);
            }

            string assetPath = $"{saveFolderPath}/{fileName}.asset";
            
            // 기존 파일이 있다면 삭제
            if (AssetDatabase.LoadAssetAtPath<ScriptDataSO>(assetPath) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
            
            AssetDatabase.CreateAsset(newData, assetPath); 

            Debug.Log($"[성공] {fileName}.asset 생성 완료! (총 {newData.dialogueLines.Count}줄의 대사 베이킹 됨)");
        }
    }

    // 파싱 함수
    static List<T> ParsingData<T>(string rawString, Func<string[], T> createFunc, params int[] allowedLengths) // params(가변 매개변수)
    {
        List<T> resultList = new List<T>();

        // effectstring가 비었으면 빈 리스트 반환
        if (string.IsNullOrEmpty(rawString))
        {
            return resultList;
        } 

        // '|' 기준으로 파싱
        string[] items = rawString.Split('|'); // ex) ADD:testValue:1|ADD:testValue2:2 -> effectDatas[0] = ADD:testValue:1

        foreach (string item in items)
        {
            // ':' 기준으로 파싱
            string[] parts = item.Split(':'); // ex) ADD:testValue:1 -> effectDataParts[0] = ADD

            // parts.Length가 allowedLengths 배열 안에 존재하는지 검사
            bool isValidLength = false;

            foreach (int length in allowedLengths)
            {
                if (parts.Length == length)
                {
                    isValidLength = true;
                    break;
                }
            }

            // 조각 개수가 요구사항과 일치할 때만 객체 생성
            if (isValidLength)
            {
                // 외부에서 전달받은 조립 규칙(createFunc)을 실행하여 리스트에 추가
                T newObject = createFunc(parts);

                if (newObject != null)
                {
                    resultList.Add(newObject);
                }
            }
        }
        return resultList;
    }

    // 딕셔너리에 키가 없으면 빈 문자열을 반환하도록 함
    private static string GetValueStrict(Dictionary<string, object> dict, string key)
    {
        return dict.ContainsKey(key) && dict[key] != null ? dict[key].ToString() : "";
    }
}

#endif