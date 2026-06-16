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
    PlayerNameInput
}

[System.Serializable]
public class ChoiceEffect
{
    public ENUM_EffectType effectType; 
    public string effectTargetPath;
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
    
    public Choice(string text, string nextID, List<ChoiceEffect> effects)
    {
        this.text = text;
        this.nextID = nextID;
        this.effects = effects;
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

    // public string Event1; // 해당 string이 뭔지에 따라 연출 시작
    // public string Event2; // 해당 string이 뭔지에 따라 연출 시작
    // public string Event3; // 해당 string이 뭔지에 따라 연출 시작

    public List<DialogueEvent> events = new List<DialogueEvent>();
    // public List<string> events = new List<string>();
    public string emotion;

    // 아래 변수들이 비워져있지 않으면 선택 연출 시작
    public List<Choice> choices = new List<Choice>();

    // public string choice1_Text;
    // public string choice1_NextID;

    // public string choice2_Text;
    // public string choice2_NextID;

    // public string choice3_Text;
    // public string choice3_NextID;
}

public class ScriptDataSO : ScriptableObject
{
    public string title;
    public List<ScriptLine> dialogueLines = new List<ScriptLine>();
}
