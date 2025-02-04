using UnityEngine;
using UnityEngine.UI;

public class ComputerUI : MonoBehaviour
{
    [SerializeField] private GameObject answerButtonObject;
    [SerializeField] private GameObject messegerHomeCanvas;
    [SerializeField] private GameObject messengerCanvas; // Messenger Canvas 오브젝트 직접 참조
    [SerializeField] private Button ToggleButton;

    private void Update() 
    {
        if(!answerButtonObject.activeSelf) // 답변이 비활성화 상태라면 토글버튼 비활성화
        {
            if(TextsController.instance?.GetIsEnd() == true)
            {
                ToggleButton.interactable = true;
            }

            else
            {
                ToggleButton.interactable = false;
            }
        }

        else if(answerButtonObject.activeSelf)
        {
            ToggleButton.interactable = true;
        }
    }

    public void ActivateMessengerHomeUI()
    {
        //Debug.Log("버튼 눌림");

        if (messegerHomeCanvas == null)
        {
            Debug.LogError("Messenger Prefab 또는 Messenger Canvas가 Inspector에서 설정되지 않았습니다.");
            return;
        }

        // Messenger Prefab 활성화
        if (!messegerHomeCanvas.activeSelf)
        {
            messegerHomeCanvas.SetActive(true);
        }
    }

    public void ActivateMessengerUI()
    {
        Debug.Log("버튼 눌림");

        if (messengerCanvas == null)
        {
            Debug.LogError("Messenger Prefab 또는 Messenger Canvas가 Inspector에서 설정되지 않았습니다.");
            return;
        }

        if (!messengerCanvas.activeSelf)
        {
            messegerHomeCanvas.SetActive(!messegerHomeCanvas.activeSelf); // 홈 UI 비활성화
            messengerCanvas.SetActive(true); // 메신저 UI 활성화
        }
    }

    public void ToggleMessengerCanvas()
    {
        if (messengerCanvas == null)
        {
            Debug.LogError("Inspector에서 Messenger Prefab이 할당되지 않았습니다.");
            return;
        }

        messegerHomeCanvas.SetActive(false);
        messengerCanvas.SetActive(false);
    }
}