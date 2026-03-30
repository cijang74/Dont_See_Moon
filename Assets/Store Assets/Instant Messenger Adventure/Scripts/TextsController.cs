﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 장면에서 표시되는 텍스트를 제어하는 클래스
public class TextsController : MonoBehaviour {

    public static TextsController instance; // 싱글턴 인스턴스 선언

    public TextGroup currentMessageGroup; // 현재 메시지 그룹을 나타냄
    public GameObject textContentPrefab; // 메시지 텍스트 프리팹
    public GameObject answerContentPrefab; // 답변 텍스트 프리팹
    public Transform messagesContent; // 메시지 컨텐츠를 배치할 부모 Transform

    public Text[] answerTexts = new Text[2]; // 답변 텍스트 배열

	private bool isEnd = false;
	private string selectAnswerText;
	

    public Text typingText; // "타이핑 중" 상태를 나타내는 텍스트

    private void Awake()
    {
        instance = this; // Awake에서 현재 객체를 싱글턴 인스턴스로 설정
    }

    // 초기화 함수
    void Start () {
        StartCoroutine(SendingMessages()); // 메시지를 보내는 코루틴 실행
    }
    
    // 메시지를 순차적으로 보내는 코루틴
    IEnumerator SendingMessages() 
    {
        for (int i = 0; i < currentMessageGroup.texts.Length; i++) 
        {
            // 각 메시지 전송 전에 대기
            yield return new WaitForSeconds(currentMessageGroup.texts[i].waitTime); 
            typingText.text = "typing..."; // "타이핑 중" 표시
            yield return new WaitForSeconds(currentMessageGroup.texts[i].typingTime); 
            typingText.text = "Online"; // "온라인" 상태로 변경
            
            // 새로운 텍스트 생성 및 설정
            GameObject newTextContent = Instantiate(textContentPrefab, messagesContent); 
            newTextContent.transform.SetSiblingIndex(0); // 메시지를 상단에 추가
            newTextContent.GetComponentInChildren<Text>().text = currentMessageGroup.texts[i].text;
            newTextContent.transform.localScale = new Vector3(1, -1, 1); // 메시지 배치를 반전
        }

        yield return new WaitForSeconds(1);

        if(currentMessageGroup.answers.Length == 0)
        {
            // 답변이 없을 경우 다른 작업 수행 후 종료
			isEnd = true;
			Debug.Log("대화 종료");
            yield break;
        }

        for (int i = 0; i < currentMessageGroup.answers.Length; i++) 
        {
            // 답변 텍스트를 설정하고 활성화
            answerTexts[i].text = currentMessageGroup.answers[i].text; 
            answerTexts[i].transform.parent.gameObject.SetActive(true); 
        }
    }

    // 다음 메시지 그룹 설정 함수
    public void SetNextGroup(int index) 
    {
		selectAnswerText = answerTexts[index].text;

        // 새로운 답변 컨텐츠 생성 및 설정
        GameObject newAnswerContent = Instantiate(answerContentPrefab, messagesContent); 
        newAnswerContent.transform.SetSiblingIndex(0); 
        newAnswerContent.GetComponentInChildren<Text>().text = currentMessageGroup.answers[index].text; 

        // 현재 메시지 그룹을 업데이트
        currentMessageGroup = currentMessageGroup.answers[index].nextGroup;

        // 이전 답변 텍스트 비활성화
        for (int i = 0; i < answerTexts.Length; i++) 
        {
            answerTexts[i].transform.parent.gameObject.SetActive(false); 
        }

        StartCoroutine(SendingMessages()); // 새로운 메시지 그룹의 메시지 전송 시작
    }

	public string GetSelectAnswerText()
	{
		return selectAnswerText;
	}

	public bool GetIsEnd()
	{
		return isEnd;
	}
}
