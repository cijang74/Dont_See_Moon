using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//classe que controla os textos que aparecem em cena

public class TextsController : MonoBehaviour {

	public static TextsController instance; 

	public TextGroup currentMessageGroup; 
	public GameObject textContentPrefab; 
	public GameObject answerContentPrefab; 
	public Transform messagesContent; 

	public Text[] answerTexts = new Text[2]; 

	public Text typingText;

	private void Awake()
	{
		instance = this; //no Awake, transforma a instancia nesse objeto
	}

	// Use this for initialization
	void Start () {

		StartCoroutine(SendingMessages()); 

	}
	


	IEnumerator SendingMessages() 
	{
		for (int i = 0; i < currentMessageGroup.texts.Length; i++) 
		{
			yield return new WaitForSeconds(currentMessageGroup.texts[i].waitTime); 
			typingText.text = "typing..."; 
			yield return new WaitForSeconds(currentMessageGroup.texts[i].typingTime); 
			typingText.text = "Online"; 
			GameObject newTextContent = Instantiate(textContentPrefab, messagesContent); 
			newTextContent.transform.SetSiblingIndex(0); 
			newTextContent.GetComponentInChildren<Text>().text = currentMessageGroup.texts[i].text;
			newTextContent.transform.localScale = new Vector3(1, -1, 1);
		}

		yield return new WaitForSeconds(1);
		if(currentMessageGroup.answers.Length == 0)
		{
			//do something else
			yield break;
		}

		for (int i = 0; i < currentMessageGroup.answers.Length; i++) 
		{
			answerTexts[i].text = currentMessageGroup.answers[i].text; 
			answerTexts[i].transform.parent.gameObject.SetActive(true); 
		}
		
	}

	public void SetNextGroup(int index) 
	{

		GameObject newAnswerContent = Instantiate(answerContentPrefab, messagesContent); 
		newAnswerContent.transform.SetSiblingIndex(0); 
		newAnswerContent.GetComponentInChildren<Text>().text = currentMessageGroup.answers[index].text; 

		currentMessageGroup = currentMessageGroup.answers[index].nextGroup;

		

		for (int i = 0; i < answerTexts.Length; i++) 
		{
			answerTexts[i].transform.parent.gameObject.SetActive(false); 
		}

		StartCoroutine(SendingMessages()); 

	}
}
