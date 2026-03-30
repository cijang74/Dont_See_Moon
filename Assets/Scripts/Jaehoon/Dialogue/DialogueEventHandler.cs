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
    [SerializeField] GameObject choicePanel;
    [SerializeField] GameObject choiceButton;

    [SerializeField] CharacterPortraitPanels characterPortraitPanels;
    [SerializeField] GameObject characterPortrait;
    [SerializeField] GameObject objectArrow; // 대사 출력이 끝났을 때 깜빡거리는 오브젝트 (ArrowBlink 스크립트가 붙어있을 곳)

    Dictionary<string, GameObject> characterPortaitDict = new Dictionary<string, GameObject>();
    // List<Image> chatacterPortraitImages = new List<Image>();

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
            DeleteCharacterObjects(speakerName); 
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
            StartCoroutine(FadeInUI(characterImage));
        }

        else
        {
            Debug.LogWarning($"초상화 이미지를 찾을 수 없습니다. 경로: Resources/{imagePath}");
        }

        // 딕셔너리 등록
        characterPortaitDict.Add(speakerName, newCharacterPortrait);

        // GameObject newCharacterPortrait;

        // switch (speakerPosition)
		// {
		// 	case ENUM_PortraitPositionType.LEFT:
		// 		newCharacterPortrait = Instantiate(characterPortrait, characterPortraitPanels.LeftCharacterPortraitPanel.transform);

        //         // 해당 부분에서 newCharacterPortrait.GetComponent<Image>().sprite로 접근하여 
        //         // 이미지를 $"Assets/5. Images/Characters/Full Illustration/{speakerName}/{speakerName}.png"파일로 교체

        //         newCharacterPortrait.transform.localPosition = Vector3.zero;
        //         newCharacterPortrait.transform.localScale = Vector3.one;
        //         newCharacterPortrait.transform.localRotation = Quaternion.identity;

        //         characterPortaitDict.Add(speakerName, newCharacterPortrait);
		// 		break;
			
		// 	case ENUM_PortraitPositionType.MIDDLE:
		// 		newCharacterPortrait = Instantiate(characterPortrait, characterPortraitPanels.MiddleCharacterPortraitPanel.transform);

        //         newCharacterPortrait.transform.localPosition = Vector3.zero;
        //         newCharacterPortrait.transform.localScale = Vector3.one;
        //         newCharacterPortrait.transform.localRotation = Quaternion.identity;

        //         characterPortaitDict.Add(speakerName, newCharacterPortrait);
		// 		break;
			
		// 	case ENUM_PortraitPositionType.RIGHT:
		// 		newCharacterPortrait = Instantiate(characterPortrait, characterPortraitPanels.RightCharacterPortraitPanel.transform);

        //         newCharacterPortrait.transform.localPosition = Vector3.zero;
        //         newCharacterPortrait.transform.localScale = Vector3.one;
        //         newCharacterPortrait.transform.localRotation = Quaternion.identity;

        //         characterPortaitDict.Add(speakerName, newCharacterPortrait);
		// 		break;
		// }
    }

    void DeleteCharacterObjects(string speakerName)
    {
        // 딕셔너리에 실제로 값이 존재하는것을 확인하면
        if (characterPortaitDict.TryGetValue(speakerName, out GameObject portraitObj))
        {
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
				// characterPortraitPanels.LeftCharacterPortraitPanel.color = visible ? Color.white : Color.gray;
				break;
			
			case ENUM_PortraitPositionType.MIDDLE:
                targetPanel = characterPortraitPanels.MiddleCharacterPortraitPanel;
				// characterPortraitPanels.MiddleCharacterPortraitPanel.color = visible ? Color.white : Color.gray;
				break;
			
			case ENUM_PortraitPositionType.RIGHT:
                targetPanel = characterPortraitPanels.RightCharacterPortraitPanel;
				// characterPortraitPanels.RightCharacterPortraitPanel.color = visible ? Color.white : Color.gray;
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
                uIFade.FadeToBlack();

                return true; // 끝났다고 알려줌
            }
        }
        return false;
    }

    // 대화 중 event를 확인하여 event를 실행시키는 메서드
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
            }
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
        
        // 최적화하려면 choices[0]만 확인하면 되고, 안정성을 높이려면 foreach로 다 돌아보는게 나음
        foreach(Choice choice in choices)
        {
            // 선택지 배열 중 하나라도 작성되어있는것이 있으면
            if(choice.text != "")
            {
                if(!isExistChoice)
                {
                    isExistChoice = true;
                    blackPanel.SetActive(true);
                }

                if(isExistChoice)
                {
                    // 선택 버튼 활성화
                    choicePanel.SetActive(true);

                    // choicePanel의 자식으로 choiceButton 인스턴싱
                    GameObject newButton = Instantiate(choiceButton, choicePanel.transform);
                    
                    // 혹시 모를 UI 변형 방지용 초기화
                    newButton.transform.localPosition = Vector3.zero; // 위치를 부모의 정중앙(0,0,0)으로
                    newButton.transform.localScale = Vector3.one; // 크기를 원래 프리팹 비율(1,1,1)로
                    newButton.transform.localRotation = Quaternion.identity; // 회전값 초기화

                    newButton.GetComponentInChildren<TextMeshProUGUI>().text = choice.text;
                    newButton.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        // 선택지 효과 (호감도 변수값 증감 등)
                        OnClickRunChoiceEffect(choice.effects);

                        // 콜백 함수로 받은, 다음 대화 라인으로 넘어가는 함수 SetNextDialog
                        OnClickSetNextDialog?.Invoke(choice.nextID);
                    });
                }
            }
        }
    }

    private void OnClickRunChoiceEffect(List<ChoiceEffect> effects)
    {
        blackPanel.SetActive(false);
        ClearChoicePanel();

        // 실제 효과 적용 (해당 코드에서는 FireStore에 저장된 값에 영향을 주도록 작성됨. DB를 사용하지 않는 프로젝트에서는 PlayerPrefs나 Singleton에 저장하면 됨.)
        foreach(ChoiceEffect effect in effects)
        {
            switch (effect.effectType)
            {
                case ENUM_EffectType.ADD:
                    // DB에 접근하여 effectTargetPath필드 값 수정
                    // FireStoreAccessManager.Instance.AddDataToDB(effect.effectTargetPath, effect.effectAmount, () =>
                    // {
                    //     // 캐시 업데이트
                    //     UserDataCachingManager.Instance.MakeUserDataCachingFromDB();
                    // });
                    break;

                case ENUM_EffectType.SET:
                    // DB에 접근하여 effectTargetPath필드 값 수정
                    // FireStoreAccessManager.Instance.SaveDataToDB<int>(effect.effectTargetPath, effect.effectAmount, () =>
                    // {
                    //     // 캐시 업데이트
                    //     UserDataCachingManager.Instance.MakeUserDataCachingFromDB();
                    // });
                    break;
                    
                case ENUM_EffectType.NONE:

                    break;
            }
        }
    }

    IEnumerator FadeInUI(Image characterImage)
    {
        float duration = 0.3f; // 0.3초 동안
        float currentTime = 0f;

        // 현재 색상 덩어리를 변수에 복사
        Color colorTarget = characterImage.color;
        
        // 시작하기 전에 알파값을 0으로 초기화
        colorTarget.a = 0f;
        characterImage.color = colorTarget; 

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            
            // 복사해둔 변수의 알파값을 조절
            colorTarget.a = Mathf.Lerp(0f, 1f, currentTime / duration);
            
            characterImage.color = colorTarget; 
            
            yield return null; // 한 프레임 대기
        }

        // 확실하게 1로 마무리
        colorTarget.a = 1f;
        characterImage.color = colorTarget; 
    }
}
