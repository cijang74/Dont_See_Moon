//***************************
// 파일명: DialogueEventHandler.cs
// 작성자: 김재훈
// 작성일: 2026.03.25
// 내용: 대화 시스템에서 호출되는 연출등을 구현하는 클래스
//***************************

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ENUM_PortraitPositionType
{
	LEFT = 0,
	MIDDLE = 1,
	RIGHT = 2
};

[System.Serializable]
public struct CharacterPortraitPanels // Speaker구조체
{
	public GameObject LeftCharacterPortraitPanel; // 초상화 이미지
	public GameObject MiddleCharacterPortraitPanel; // 초상화 이미지
	public GameObject RightCharacterPortraitPanel; // 초상화 이미지
}

public class DialogueEventHandler : MonoBehaviour
{
    [SerializeField] UIFade uIFade; // UI Fade 효과 구현된 클래스

    [SerializeField] GameObject blackPanel;
    [SerializeField] GameObject dialoguePanel; //대화 UI 전체 오브젝트
    [SerializeField] GameObject dialogueCanvas; //대화 UI 전체 오브젝트
    [SerializeField] GameObject choicePanel;
    [SerializeField] GameObject choiceButton;

    [SerializeField] Image backgroundImage;

    [SerializeField] CharacterPortraitPanels characterPortraitPanels;
    [SerializeField] GameObject characterPortrait;
    [SerializeField] GameObject objectArrow; // 대사 출력이 끝났을 때 깜빡거리는 오브젝트 (ArrowBlink 스크립트가 붙어있을 곳)

    Dictionary<string, GameObject> characterPortaitDict = new Dictionary<string, GameObject>();

    [SerializeField] GameObject votePanel; // 투표 UI 전체 패널
    [SerializeField] Transform voteButtonGroup; // 버튼들이 자식으로 생성될 부모 (Vertical Layout Group 등이 붙은 곳)
    [SerializeField] GameObject voteButtonPrefab; // 인스펙터에서 넣을 버튼 프리팹

    // 외부에서 현재 투표 중인지 확인할 수 있는 플래그
    public bool isVoting = false; 

    // 투표 대상 캐릭터 이름들
    private string[] voteCandidates = { "James", "Nicholas", "Ella", "Sophia" };

    // 씬 처음 로드할 때 호출할 UI 세팅 메서드
    public void UISetup()
	{
        characterPortaitDict.Clear();

		// 일단 화살표 커서 비활성화, 모든 대화 화자들 어둡게 처리
		AdjustGrayObjects(ENUM_PortraitPositionType.LEFT, false);
		AdjustGrayObjects(ENUM_PortraitPositionType.MIDDLE, false);
		AdjustGrayObjects(ENUM_PortraitPositionType.RIGHT, false);

		// 화면에 배치된 캐릭터 이미지 자체를 보이지 않도록 수정
		characterPortraitPanels.LeftCharacterPortraitPanel.SetActive(false);
		characterPortraitPanels.MiddleCharacterPortraitPanel.SetActive(false);
		characterPortraitPanels.RightCharacterPortraitPanel.SetActive(false);
	}

    // 특정 위치의 초상화를 보이지 않도록 처리하는 메서드
	void SetActiveObjects(ENUM_PortraitPositionType speakerPosition, bool visible)
	{
		switch (speakerPosition)
		{
			case ENUM_PortraitPositionType.LEFT:
				characterPortraitPanels.LeftCharacterPortraitPanel.SetActive(visible);
				break;
			
			case ENUM_PortraitPositionType.MIDDLE:
				characterPortraitPanels.MiddleCharacterPortraitPanel.SetActive(visible);
				break;
			
			case ENUM_PortraitPositionType.RIGHT:
				characterPortraitPanels.RightCharacterPortraitPanel.SetActive(visible);
				break;
		}
	}

    void AppearCharacterObjects(ENUM_PortraitPositionType speakerPosition, string speakerName)
    {
        // 딕셔너리에 이미 해당 캐릭터가 존재하면 기존 것 삭제
        if (characterPortaitDict.ContainsKey(speakerName))
        {
            StartCoroutine(DeleteCharacterObjects(speakerName)); 
        }

        Transform parentPanel = null;

        // speakerPosition에 따라 부모 패널 결정
        switch (speakerPosition)
        {
            case ENUM_PortraitPositionType.LEFT: 
                parentPanel = characterPortraitPanels.LeftCharacterPortraitPanel.transform; 
                break;

            case ENUM_PortraitPositionType.MIDDLE: 
                parentPanel = characterPortraitPanels.MiddleCharacterPortraitPanel.transform; 
                break;

            case ENUM_PortraitPositionType.RIGHT: 
                parentPanel = characterPortraitPanels.RightCharacterPortraitPanel.transform; 
                break;
        }

        // 인스턴스 및 초기화
        GameObject newCharacterPortrait = Instantiate(characterPortrait, parentPanel);
        newCharacterPortrait.transform.localPosition = Vector3.zero;
        newCharacterPortrait.transform.localScale = Vector3.one;
        newCharacterPortrait.transform.localRotation = Quaternion.identity;

        // 이미지 교체
        string imagePath = $"Characters/Full Illustration/{speakerName}/{speakerName}";
        Sprite loadedSprite = Resources.Load<Sprite>(imagePath);

        // 이미지 잘 찾았으면 Image 컴포넌트 접근하여 스프라이트 교체
        if (loadedSprite != null)
        {
            Image characterImage = newCharacterPortrait.GetComponent<Image>();
            characterImage.sprite = loadedSprite;
            StartCoroutine(FadeUI(characterImage, 0, 1));
        }

        else
        {
            Debug.LogWarning($"초상화 이미지를 찾을 수 없습니다. 경로: Resources/{imagePath}");
        }

        // 딕셔너리 등록
        characterPortaitDict.Add(speakerName, newCharacterPortrait);
    }

    IEnumerator DeleteCharacterObjects(string speakerName)
    {
        // 딕셔너리에 실제로 값이 존재하는것을 확인하면
        if (characterPortaitDict.TryGetValue(speakerName, out GameObject portraitObj))
        {
            // 사라지는 코루틴 실행 뒤
            yield return StartCoroutine(FadeUI(characterPortaitDict[speakerName].GetComponent<Image>(), 1, 0));

            // speakerName을 키값으로 가진 오브젝트 삭제
            Destroy(characterPortaitDict[speakerName]);

            // 딕셔너리에서도 삭제
            characterPortaitDict.Remove(speakerName);
        }
    }

    // 특정 위치의 초상화를 어둡게 처리하는 메서드
	void AdjustGrayObjects(ENUM_PortraitPositionType speakerPosition, bool visible)
	{
		// 화살표는 텍스트 출력이 모두 끝났을 때만 켜져야 하므로 상태가 바뀔 땐 항상 꺼둡니다.
		SetActiveArrow(false);

        GameObject targetPanel = null;

        // 수정 필요!!
		switch (speakerPosition)
		{
			case ENUM_PortraitPositionType.LEFT:
                targetPanel = characterPortraitPanels.LeftCharacterPortraitPanel;
				break;
			
			case ENUM_PortraitPositionType.MIDDLE:
                targetPanel = characterPortraitPanels.MiddleCharacterPortraitPanel;
				break;
			
			case ENUM_PortraitPositionType.RIGHT:
                targetPanel = characterPortraitPanels.RightCharacterPortraitPanel;
				break;
		}

        // 정상적으로 targetPanel을 가져왔으면
        if (targetPanel != null)
        {
            // 켤 때는 흰색(원래 색), 끌 때는 회색
            Color targetColor = visible ? Color.white : Color.gray;

            // 패널 자식 오브젝트들을 하나씩 확인
            foreach (Transform child in targetPanel.transform)
            {
                // GetComponentsInChildren을 사용해 프리팹 안의 모든 Image를 한 번에 가져오기
                Image[] images = child.GetComponentsInChildren<Image>();
                
                foreach (Image image in images)
                {
                    image.color = targetColor;
                }
            }
        }
	}

    // 대화 이벤트 리스트에 Vote가 있는지 검사
    public bool CheckVoteEvent(List<DialogueEvent> events)
    {
        foreach (DialogueEvent eventData in events)
        {
            if (eventData.eventType == ENUM_EventType.Vote)
            {
                return true;
            }
        }
        return false;
    }

    // 동적으로 버튼을 생성하고 띄워주는 메서드
    public void ShowVoteUI(Action<string> onVoteButtonClicked)
    {
        isVoting = true;
        blackPanel.SetActive(true); // 배경 어둡게
        votePanel.SetActive(true);  // 투표 창 활성화

        // 1. 기존에 생성되어 있던 버튼 찌꺼기들 깔끔하게 청소
        foreach (Transform child in voteButtonGroup)
        {
            Destroy(child.gameObject);
        }

        // 2. 캐릭터 수만큼 투표 버튼 동적 생성
        foreach (string candidate in voteCandidates)
        {
            // 부모 밑에 프리팹 인스턴싱
            GameObject newButton = Instantiate(voteButtonPrefab, voteButtonGroup);
            
            // UI 스케일/위치 꼬임 방지용 초기화
            newButton.transform.localPosition = Vector3.zero;
            newButton.transform.localScale = Vector3.one;
            newButton.transform.localRotation = Quaternion.identity;

            // 버튼 텍스트를 캐릭터 이름으로 변경
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = candidate;

            // 💡 코드로 OnClick 이벤트 연결
            newButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                // DialogueSystem에서 넘겨준 처리 함수 실행 (누구에게 투표했는지 문자열 전달)
                onVoteButtonClicked?.Invoke(candidate);
            });
        }
    }

    // 투표가 끝났을 때 UI를 닫기 위한 메서드
    public void HideVoteUI()
    {
        isVoting = false;
        blackPanel.SetActive(false);
        votePanel.SetActive(false);
    }

    public void SetActiveArrow(bool visible)
    {
        objectArrow.SetActive(visible);
    }

    public void CheckAndAdjustSpeeakerGray(string speakerPosition)
    {
        switch (speakerPosition)
		{
			case "Left":
				AdjustGrayObjects(ENUM_PortraitPositionType.LEFT, true);
				AdjustGrayObjects(ENUM_PortraitPositionType.MIDDLE, false);
				AdjustGrayObjects(ENUM_PortraitPositionType.RIGHT, false);
				break;
				
			case "Middle":
				AdjustGrayObjects(ENUM_PortraitPositionType.LEFT, false);
				AdjustGrayObjects(ENUM_PortraitPositionType.MIDDLE, true);
				AdjustGrayObjects(ENUM_PortraitPositionType.RIGHT, false);
				break;

			case "Right":
				AdjustGrayObjects(ENUM_PortraitPositionType.LEFT, false);
				AdjustGrayObjects(ENUM_PortraitPositionType.MIDDLE, false);
				AdjustGrayObjects(ENUM_PortraitPositionType.RIGHT, true);
				break;
		}
    }

    // 대사 종료 시 event에 End가 포함되어있는지 체크하는 메서드
    public bool CheckEndEvent(List<DialogueEvent> events)
    {
        // 이벤트 뒤져보고
        foreach(DialogueEvent eventData in events)
        {
            // 이벤트 종류가 End인것이 있으면 UI 정리
            if(eventData.eventType == ENUM_EventType.End)
            {
                AdjustGrayObjects(ENUM_PortraitPositionType.LEFT, false);
                AdjustGrayObjects(ENUM_PortraitPositionType.MIDDLE, false);
                AdjustGrayObjects(ENUM_PortraitPositionType.RIGHT, false);

                characterPortraitPanels.LeftCharacterPortraitPanel.gameObject.SetActive(false);
                characterPortraitPanels.MiddleCharacterPortraitPanel.gameObject.SetActive(false);
                characterPortraitPanels.RightCharacterPortraitPanel.gameObject.SetActive(false);
                
                dialoguePanel.SetActive(false);
                dialogueCanvas.SetActive(false);
                // uIFade.FadeToBlack();

                return true; // 끝났다고 알려줌
            }
        }
        return false;
    }

    // 대화 중 event를 확인하여 event를 실행시키는 메서드 (해당 메서드 내 이벤트들은 대화 타이핑이 시작하자마자 실행되는 이벤트들임.)
    public void CheckAndRunEvent(List<DialogueEvent> events)
    {
        foreach(DialogueEvent eventData in events)
        {
            Debug.Log($"Current Event: {eventData.eventType}");

            switch (eventData.eventType)
            {
                case ENUM_EventType.Appear_Left:
                    SetActiveObjects(ENUM_PortraitPositionType.LEFT, true);
					AppearCharacterObjects(ENUM_PortraitPositionType.LEFT, eventData.target);
					break;
				
				case ENUM_EventType.Appear_Middle:
                    SetActiveObjects(ENUM_PortraitPositionType.MIDDLE, true);
					AppearCharacterObjects(ENUM_PortraitPositionType.MIDDLE, eventData.target);
					break;
				
				case ENUM_EventType.Appear_Right:
                    SetActiveObjects(ENUM_PortraitPositionType.RIGHT, true);
					AppearCharacterObjects(ENUM_PortraitPositionType.RIGHT, eventData.target);
					break;

                case ENUM_EventType.Out:
                    StartCoroutine(DeleteCharacterObjects(eventData.target));
                    break;

                case ENUM_EventType.BackGroundChange:
                    ChangeBackGroundImage(eventData.target);
                    break;
            }
        }
    }

    void ChangeBackGroundImage(string backgroundName)
    {
        // 이미지 교체
        string imagePath = $"BackGroundImages/{backgroundName}";
        Sprite loadedSprite = Resources.Load<Sprite>(imagePath);

        // 이미지 잘 찾았으면 Image 컴포넌트 접근하여 스프라이트 교체
        if (loadedSprite != null)
        {
            backgroundImage.sprite = loadedSprite;
            StartCoroutine(FadeUI(backgroundImage, 0, 1));
        }

        else
        {
            Debug.LogWarning($"초상화 이미지를 찾을 수 없습니다. 경로: Resources/{imagePath}");
        }
    }

    void ClearChoicePanel()
    {
        // 기존 choicePanel에 있던 찌꺼기들 없애주기
        foreach (Transform child in choicePanel.transform)
        {
            Destroy(child.gameObject);
        }
        
        choicePanel.SetActive(false);
    }

    public bool CehckChoiceEvent(List<Choice> choices)
    {
        bool isExistChoice = false;
        
        // 최적화하려면 choices[0]만 확인하면 되고, 안정성을 높이려면 foreach로 다 돌아보는게 나음
        foreach(Choice choice in choices)
        {
            // 선택지 배열 중 하나라도 작성되어있는것이 있으면
            if(choice.text != "")
            {
                if(!isExistChoice)
                {
                    isExistChoice = true;
                }
            }
        }

        return isExistChoice;
    }

    public void CheckAndSetActiveChoiceButton(List<Choice> choices, Action<string> OnClickSetNextDialog)
    {
        bool isExistChoice = false;
        
        foreach(Choice choice in choices)
        {
            if(choice.text != "")
            {
                if(!isExistChoice)
                {
                    isExistChoice = true;
                    blackPanel.SetActive(true);
                }

                if(isExistChoice)
                {
                    choicePanel.SetActive(true);

                    // 버튼 인스턴싱 및 초기화
                    GameObject newButton = Instantiate(choiceButton, choicePanel.transform);
                    newButton.transform.localPosition = Vector3.zero;
                    newButton.transform.localScale = Vector3.one; 
                    newButton.transform.localRotation = Quaternion.identity; 

                    TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
                    buttonText.text = choice.text;

                    Button btnComponent = newButton.GetComponent<Button>();

                    // 💡 [핵심 검사 로직] 요구 증거가 None이 아닌데, EvidenceManager에 그 증거가 없다면?
                    if (choice.requiredEvidence != InteractionObjectType.None && !EvidenceManager.Instance.HasEvidence(choice.requiredEvidence))
                    {
                        // 버튼을 누를 수 없게 비활성화하고, 텍스트 색상을 어둡게 처리
                        btnComponent.interactable = false;
                        buttonText.text = $"<color=#808080>{choice.text} (증거 부족)</color>"; 
                    }
                    else
                    {
                        // 조건이 없거나 만족했을 경우 기존처럼 리스너 등록
                        btnComponent.onClick.AddListener(() =>
                        {
                            OnClickRunChoiceEffect(choice.effects);
                            OnClickSetNextDialog?.Invoke(choice.nextID);
                        });
                    }
                }
            }
        }
    }

    private void OnClickRunChoiceEffect(List<ChoiceEffect> effects)
    {
        blackPanel.SetActive(false);
        ClearChoicePanel();

        foreach(ChoiceEffect effect in effects)
        {
            switch (effect.effectType)
            {
                case ENUM_EffectType.ADD:
                    // 💡 [추가] 주체의 대상에 대한 의심도 증가/감소
                    CharacterStatusManager.Instance.AddSuspicion(effect.effectSubject, effect.effectTarget, effect.effectAmount);
                    Debug.Log($"[효과] {effect.effectSubject}의 {effect.effectTarget}에 대한 의심도가 {effect.effectAmount}만큼 증가했습니다.");
                    break;

                case ENUM_EffectType.SET:
                    // 💡 [추가] 주체의 대상에 대한 의심도 특정 수치로 고정
                    CharacterStatusManager.Instance.SetSuspicion(effect.effectSubject, effect.effectTarget, effect.effectAmount);
                    Debug.Log($"[효과] {effect.effectSubject}의 {effect.effectTarget}에 대한 의심도가 {effect.effectAmount}로 설정되었습니다.");
                    break;
                    
                case ENUM_EffectType.NONE:
                    break;
            }
        }
    }

    IEnumerator FadeUI(Image characterImage, float startAlpha, float targetAlpha)
    {
        // 삭제된 이미지 접근할 수 있으므로 오브젝트 파괴 시 코루틴 종료
        if (characterImage == null) yield break;

        float duration = 0.3f; // 0.3초 동안
        float currentTime = 0f;

        // 현재 색상 덩어리를 변수에 복사
        Color colorTarget = characterImage.color;
        
        // 시작하기 전에 알파값을 startAlpha으로 초기화
        colorTarget.a = startAlpha;
        characterImage.color = colorTarget; 

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;

            // 루프 도중 클릭 시 기존 이미지를 삭제하기 때문에 루프 도중 삭제된 이미지 접근할 수 있으므로 오브젝트 파괴 시 코루틴 종료
            if (characterImage == null) yield break;
            
            // 복사해둔 변수의 알파값을 조절
            colorTarget.a = Mathf.Lerp(startAlpha, targetAlpha, currentTime / duration);
            
            characterImage.color = colorTarget; 
            
            yield return null; // 한 프레임 대기
        }

        // 확실하게 targetAlpha로 마무리
        if (characterImage != null)
        {
            colorTarget.a = targetAlpha;
            characterImage.color = colorTarget; 
        }
    }
}
