//***************************
// 파일명: DialogueSystem.cs
// 작성자: 김재훈
// 작성일: 2026.03.24
// 내용: 대화 시스템
//***************************

using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI speakerNameText; // 캐릭터의 이름이 출력될 Text UI
	[SerializeField] TextMeshProUGUI dialogueText; // 실제 대사 내용이 출력될 Text UI

	[SerializeField] bool isAutoStart = true; // true일 경우, UpdateDialog가 처음 호출될 때 첫 대사가 자동 시작

	[SerializeField] ScriptDataLoader scriptDataLoader;
	[SerializeField] InputAndCheckPointingUI inputAndCheckPointingUI;
	[SerializeField] DialogueEventHandler dialogueEventHandler;

	// [SerializeField] GameObject foldingUIPanel;
	[SerializeField] GameObject DialoguePanel;

	// [SerializeField] GameObject ActiveContinuousProgressButton;
	// [SerializeField] GameObject DeActiveContinuousProgressButton;
	// [SerializeField] GameObject ActiveAutoProgressButton;
	// [SerializeField] GameObject DeActiveAutoProgressButton;
	// [SerializeField] ButtonController buttonController;

	[SerializeField] GameObject titlePopup;
	[SerializeField] TMP_Text titlePopupText;

	[SerializeField] float touchCooldown = 0.2f; // 터치 딜레이 시간
	float lastTouchTime = 0f;

	Dictionary<string, ScriptLine> dialogueDict = new Dictionary<string, ScriptLine>();

	bool isFirst = true; // 최초 실행시 초기화시키는 플래그
	// bool isTitlePopupEnd = false;

	float autoProgressTime = 3.0f;
	bool isAutoCountStart = false;
	bool isAutoCountEnd = false;

	float typingSpeed = 0.1f;
	bool isTypingEffectRunning = false;
	// bool isNameInputTriggered = false; // 이름 입력 패널을 띄웠었는지 확인하는 변수

	// 대화 이어주는 용도로 사용되는 ID 변수들
	string currentDialogueID = "";
	string nextDialogueID = "0";

	string currentSpeakerName = "";
	string currentListenerName = "";

    void Start()
	{
		this.dialogueDict = scriptDataLoader.dialogueDict;

		// // 타이틀 팝업 띄워주기
		// if (titlePopup != null)
		// {
		// 	titlePopupText.text = scriptDataLoader.title;
		// 	yield return StartCoroutine(InOutRoutine(titlePopup, titlePopupText, 2f));
		// }

		// Debug.Log("끝났음");

		// 씬 로드 시 초기 UI 세팅
		// foldingUIPanel.SetActive(true);
		DialoguePanel.SetActive(true);
		dialogueEventHandler.UISetup();

		// 버튼 UI 세팅
		// if(CurrentSelectDataManager.Instance != null)
		// {
		// 	// 연속 진행 버튼 체크되어있었다면
		// 	if(CurrentSelectDataManager.Instance.storyContinuousProgress)
		// 	{
		// 		ActiveContinuousProgressButton.SetActive(false);
		// 		DeActiveContinuousProgressButton.SetActive(true);
		// 	}

		// 	if(!CurrentSelectDataManager.Instance.storyContinuousProgress)
		// 	{
		// 		ActiveContinuousProgressButton.SetActive(true);
		// 		DeActiveContinuousProgressButton.SetActive(false);
		// 	}

		// 	// 자동 진행 버튼 체크되어있었다면
		// 	if(CurrentSelectDataManager.Instance.storyAutoProgress)
		// 	{
		// 		ActiveAutoProgressButton.SetActive(false);
		// 		DeActiveAutoProgressButton.SetActive(true);
		// 	}

		// 	if(!CurrentSelectDataManager.Instance.storyAutoProgress)
		// 	{
		// 		ActiveAutoProgressButton.SetActive(true);
		// 		DeActiveAutoProgressButton.SetActive(false);
		// 	}

		// 	if(CurrentSelectDataManager.Instance.storyHideUI)
		// 	{
		// 		CurrentSelectDataManager.Instance.storyHideUI = false;
		// 	}
		// }

		// isTitlePopupEnd = true;
	}

    // 외부(예: DialogTest)에서 대화 진행 상태를 체크하기 위해 매 프레임 호출하는 메서드, 모든 대화가 종료되었으면 true, 아직 대화가 진행 중이면 false 반환.
	public bool UpdateDialog()
	{
		// if(!isTitlePopupEnd)
		// {
		// 	return false;
		// }

		// 대화 시작될 때 최초 1회 초기화
		if ( isFirst == true )
		{
			dialogueEventHandler.UISetup();

			// 자동 재생 옵션이 켜져있다면 첫 번째 대사 세팅을 즉시 시작
			if ( isAutoStart )
            {   
                SetNextDialog();
            }
			isFirst = false;

			return false; // 첫 세팅을 한 프레임에서는 아래의 터치 입력 생까도록
		}

		// 터치 입력 처리
		// if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
		if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame) 
		{
			// 터치 쿨타임 체크
			if (Time.time - lastTouchTime < touchCooldown)
			{
				return false; 
			}

			lastTouchTime = Time.time; // 쿨타임 지났으면 갱신

			// 설정창이나 버튼등을 터치한것이면 씹기
			if(inputAndCheckPointingUI.IsPointingUI())
			{
				return false;
			}

			// UI 숨기기 상태일 때 터치한것이면 씹기
			if(CurrentSelectDataManager.Instance.storyHideUI)
			{
				return false;
			}

			// 터치했으면 자동 카운트다운 중지
			StopCoroutine("AutoProgress");

			// 카운트다운 변수 초기화
			isAutoCountStart = false; 
        	isAutoCountEnd = false;

			// 텍스트가 한 글자씩 나오는 타이핑 효과 도중에 클릭한 경우
			if ( isTypingEffectRunning == true )
			{
				isTypingEffectRunning = false;
				
				// 진행 중이던 타이핑 코루틴을 강제로 중지
				StopCoroutine("OnTypingText");
                
				// 연출을 생략하고 현재 대사 전체를 즉시 출력
				dialogueText.text = dialogueDict[currentDialogueID].dialogueText;

				// 대사가 끝났으므로 깜빡이는 커서 활성화
				dialogueEventHandler.SetActiveArrow(true);

				// 대화 내 Choice가 존재하면 Choice 버튼 생성
				dialogueEventHandler.CheckAndSetActiveChoiceButton(dialogueDict[currentDialogueID].choices, SetNextDialog);

				return false;
			}

			//=========아래부터는 타이핑 효과가 끝나고 클릭한 경우 실행됨=========
			// if(!dialogueEventHandler.isEndPlayerNameInput) // 이름 입력이 아직 안끝났으면 다른 터치 모두 무시
			// {
			// 	return false;
			// }

			// 선택지가 있는지 체크하고 존재하면
			bool hasChoices = dialogueEventHandler.CehckChoiceEvent(dialogueDict[currentDialogueID].choices);
            if (hasChoices)
            {
				// 다음 대사로 넘어가지 않고 씹기
                return false; 
            }

			// if(!isNameInputTriggered) // 아직 이름 입력 창을 띄운 적이 없으면 이름 입력 이벤트가 발생했는지 체크
			// {
			// 	bool hasNameInput = dialogueEventHandler.CheckPlayerNameInputEvent(dialogueDict[currentDialogueID].events);
				
			// 	// 만약 이벤트가 있어서 창을 띄웠다면
			// 	if(hasNameInput)
			// 	{
			// 		isNameInputTriggered = true; // 띄웠다고 표시
			// 		return false; // 다음 대사로 넘어가지 않고 씹기
			// 	}
			// }

			// 대화 내 End이벤트가 존재하는지 체크, 존재하면 UI 정리
			Debug.Log($"Current DialogueID: {currentDialogueID}");
			bool isDialogueEnd = dialogueEventHandler.CheckEndEvent(dialogueDict[currentDialogueID].events);

			// End이벤트가 존재하면 대화도 종료
			if(isDialogueEnd)
			{
				if (CurrentSelectDataManager.Instance != null)
				{
					// 현재 스토리 정보 계산
					int currentStoryNum = CurrentSelectDataManager.Instance.currentSelectStoryNum;
					int currentChapter = (currentStoryNum / 10) + 1;
					int currentEpisode = currentStoryNum % 10;

					// 다음 스토리 정보 계산
					int nextStoryNum = currentStoryNum + 1;
					int nextChapter = (nextStoryNum / 10) + 1;
					int nextEpisode = nextStoryNum % 10;

					// DB에 접근하여 effectTargetPath필드 값 수정
					// FireStoreAccessManager.Instance.SaveDataToDB($"chapterList.{currentChapter}.Episodes.{currentEpisode}.isWatched", true, () =>
					// {
					// 	FireStoreAccessManager.Instance.SaveDataToDB($"chapterList.{nextChapter}.Episodes.{nextEpisode}.isUnlocked", true, () =>
					// 	{
					// 		// 캐시 업데이트
					// 		UserDataCachingManager.Instance.MakeUserDataCachingFromDB();
					// 	});
					// });
				}
				return true; // 대화 종료
			}

			// 대화 끝내라는 요청을 하는 이벤트가 없었다면 다음 대화 출력
			SetNextDialog();
		}

		else
		{
			// 자동 진행 옵션이 켜져있으면서, 터치 안하고 있을 경우
			if(CurrentSelectDataManager.Instance != null)
			{
				if(CurrentSelectDataManager.Instance.storyAutoProgress)
				{
					// 카운트다운 끝났을 경우 다음 대화로 넘어가도록 하기
					if(isAutoCountEnd)
					{
						isAutoCountEnd = false;
						isAutoCountStart = false;

						// 대화 내 End이벤트가 존재하는지 체크, 존재하면 UI 정리
						Debug.Log($"Current DialogueID: {currentDialogueID}");
						bool isDialogueEnd = dialogueEventHandler.CheckEndEvent(dialogueDict[currentDialogueID].events);

						// End이벤트가 존재하면 대화도 종료
						if(isDialogueEnd)
						{
							return true; // 대화 종료
						}

						// 대화 끝내라는 요청을 하는 이벤트가 없었다면 다음 대화 출력
						SetNextDialog();
					}

					// 선택지가 있는지 확인
					bool hasChoices = dialogueEventHandler.CehckChoiceEvent(dialogueDict[currentDialogueID].choices);

					// 선택지가 없는 대화일떄만 카운트다운 작동
					if(!hasChoices)
					{
						// 카운트다운 시작 안했고, 타이핑중이 아니라면
						if(!isAutoCountStart && !isTypingEffectRunning)
						{
							isAutoCountStart = true;

							// 자동 진행 카운트다운 3초
							StartCoroutine("AutoProgress");
						}
					}

				}
			}

			else
			{
				Debug.Log("CurrentSelectDataManager접근불가");
			}
		}
		return false;
	}

    // 다음 대사를 화면에 설정하고 연출을 시작하는 메서드
	void SetNextDialog(string _nextID = "")
	{
		// 최근 화자랑 청자 세팅
		if(currentDialogueID != "")
		{
			if(dialogueDict[currentDialogueID].speakerName != "")
			{
				currentSpeakerName = dialogueDict[currentDialogueID].speakerName;
			}

			if(dialogueDict[currentDialogueID].listenerName != "")
			{
				currentListenerName = dialogueDict[currentDialogueID].listenerName;
			}
		}

		// csv파일에 빈 칸이 있으면 규칙에 따라 자동으로 변수 채우기

		//=========================================================
		// 파라미터로 넘겨받은 다음 대화 라인 ID가 존재하지 않는다면
		if(_nextID == "")
		{
			// nextDilogueID가 공백일경우 최근 대화 ID + 1로 자동 수정
			if(nextDialogueID == "")
			{
				nextDialogueID = (Convert.ToInt32(currentDialogueID) + 1).ToString();
			}
		}

		// // 파라미터로 넘겨받은 다음 대화 라인 ID가 존재한다면
		else
		{
			nextDialogueID = _nextID;
		}


		// 현재 대사 ID 업데이트
		currentDialogueID = nextDialogueID;
		//=========================================================

		// 이번 대사의 이벤트를 확인하고 있으면 실행
		dialogueEventHandler.CheckAndRunEvent(dialogueDict[currentDialogueID].events);

		// 화자 초상화 위치를 제외하고 나머지 초상화 위치는 어둡게 처리
		dialogueEventHandler.CheckAndAdjustSpeeakerGray(dialogueDict[currentDialogueID].speakerPosition);

		//=========================================================
		// 공백이면 최근 화자 이름 쓰기
		if(dialogueDict[currentDialogueID].speakerName == "")
		{
			speakerNameText.text = currentSpeakerName;
		}

		// 공백이 아니면 데이터에 설정된 이름으로 이름표 텍스트를 변경
		else
		{
			speakerNameText.text = dialogueDict[currentDialogueID].speakerName;
		}
		//=========================================================

		// 다음 대사 ID 업데이트
		nextDialogueID = dialogueDict[currentDialogueID].nextID;
		
		// 텍스트 타이핑 코루틴을 시작
		StartCoroutine("OnTypingText");
	}

    // 텍스트를 한 글자씩 타자기처럼 출력하는 코루틴
	IEnumerator OnTypingText()
	{
		int index = 0;
		
		isTypingEffectRunning = true; // 타이핑 연출 시작

		// 현재 대사의 전체 글자 수만큼 반복
		while ( index <= dialogueDict[currentDialogueID].dialogueText.Length )
		{
			// Substring을 이용해 처음부터 index 개수만큼의 문자열만 잘라내어 화면에 그리기.
			dialogueText.text = dialogueDict[currentDialogueID].dialogueText.Substring(0, index);

			index ++;
		
			// 설정된 타이핑 속도(typingSpeed)만큼 기다렸다가 다음 루프(글자)로 이동
			yield return new WaitForSeconds(typingSpeed);
		}

		isTypingEffectRunning = false; // 타이핑 연출 종료 표시

		// 대사 출력이 완료 커서(화살표) 활성화
		dialogueEventHandler.SetActiveArrow(true);

		// 대화 내 Choice가 존재하면 Choice 버튼 생성
		dialogueEventHandler.CheckAndSetActiveChoiceButton(dialogueDict[currentDialogueID].choices, SetNextDialog);
	}

	IEnumerator AutoProgress()
	{
		Debug.Log("카운트다운 타이머 작동");
		yield return new WaitForSeconds(autoProgressTime);
		isAutoCountEnd = true;
	}

	IEnumerator InOutRoutine(GameObject popUpToOpen, TMP_Text popUpToOpenText, float time)
    {
        // popUpToOpen.SetActive(true);
		yield return StartCoroutine(FadeUI(popUpToOpen, popUpToOpenText, 0, 1));

        yield return new WaitForSeconds(time);

        // popUpToOpen.SetActive(false);
		yield return StartCoroutine(FadeUI(popUpToOpen, popUpToOpenText, 1, 0));
    }

	IEnumerator FadeUI(GameObject popUpToOpen, TMP_Text popUpToOpenText, float startAlpha, float targetAlpha)
    {
		// 목표 알파값이 1이면 오브젝트 켜주고 시작
		if (targetAlpha >= 1f && popUpToOpen != null)
		{
			popUpToOpen.SetActive(true);
		}

        float duration = 0.3f; // 0.3초 동안
        float currentTime = 0f;

		Image PopUpFrameImage = popUpToOpen.GetComponent<Image>();

		if (PopUpFrameImage == null || popUpToOpenText == null) yield break;

        // 현재 색상 덩어리를 변수에 복사
        Color colorTargetFrame = PopUpFrameImage.color;
		Color colorTargetText = popUpToOpenText.color;
        
        // 시작하기 전에 알파값을 0으로 초기화
        colorTargetFrame.a = startAlpha;
		colorTargetText.a = startAlpha;
        PopUpFrameImage.color = colorTargetFrame; 
		popUpToOpenText.color = colorTargetText;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;

			if (PopUpFrameImage == null || popUpToOpenText == null) yield break;
            
            // 복사해둔 변수의 알파값을 조절
            colorTargetFrame.a = Mathf.Lerp(startAlpha, targetAlpha, currentTime / duration);
            colorTargetText.a = Mathf.Lerp(startAlpha, targetAlpha, currentTime / duration);
            
            PopUpFrameImage.color = colorTargetFrame; 
            popUpToOpenText.color = colorTargetText; 
            
            yield return null; // 한 프레임 대기
        }

        // 확실하게 1로 마무리
        if (PopUpFrameImage != null && popUpToOpenText != null)
        {
            colorTargetFrame.a = targetAlpha;
            colorTargetText.a = targetAlpha;
            PopUpFrameImage.color = colorTargetFrame;
			popUpToOpenText.color = colorTargetText;
        }

		// 목표 알파값이 0이면 아예 오브젝트 꺼주기
		if (targetAlpha <= 0f && popUpToOpen != null)
		{
			popUpToOpen.SetActive(false);
		}
    }
}
