using UnityEngine;
using System.Collections.Generic;

public enum ENUM_EffectType
{
    NONE,
    ADD,
    SET
}

public enum ENUM_EventType
{
    Appear_Left,
    Appear_Middle,
    Appear_Right,
    Out,
    End,
    BackGroundChange,
    PlayerNameInput,
    Vote
}

[System.Serializable]
public class ChoiceEffect
{
    public ENUM_EffectType effectType; 
    
    // 💡 변경: 문자열 경로 대신 의심하는 주체와 대상을 Enum으로 저장
    public InteractionObjectType effectSubject; // 의심하는 주체 (예: James)
    public InteractionObjectType effectTarget;  // 의심받는 대상 (예: Ella)
    
    public int effectAmount;
}

[System.Serializable]
public class DialogueEvent
{
    public ENUM_EventType eventType; 
    public string target;
}

[System.Serializable]
public class Choice
{
    public string text;
    public string nextID;

    public List<ChoiceEffect> effects = new List<ChoiceEffect>();
    
    // 💡 [추가] 이 선택지를 활성화하기 위해 필요한 증거 타입 (기본값 None)
    public InteractionObjectType requiredEvidence = InteractionObjectType.None;
    
    // 💡 [수정] 생성자에 requiredEvidence 추가
    public Choice(string text, string nextID, List<ChoiceEffect> effects, InteractionObjectType requiredEvidence)
    {
        this.text = text;
        this.nextID = nextID;
        this.effects = effects;
        this.requiredEvidence = requiredEvidence;
    }
}

// 대사 1줄의 데이터를 담는 클래스
[System.Serializable]
public class ScriptLine
{
    public string ID; // id가 "1-1" 이런 경우를 대비하여 string으로 사용
    public string speakerPosition;

    public string speakerName;
    public string listenerName;

    public string dialogueText;

    public string nextID; // 비워져 있으면 자동으로 다음 대화 출력

    public List<DialogueEvent> events = new List<DialogueEvent>();
    public string emotion;

    // 아래 변수들이 비워져있지 않으면 선택 연출 시작
    public List<Choice> choices = new List<Choice>();
}

public class ScriptDataSO : ScriptableObject
{
    public string title;
    public List<ScriptLine> dialogueLines = new List<ScriptLine>();
}
