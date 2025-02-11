using UnityEngine;
using UnityEngine.UI;

public class ComputerUI : MonoBehaviour
{
    [SerializeField] private GameObject answerButtonObject_M;
    [SerializeField] private GameObject answerButtonObject_F;
    [SerializeField] private GameObject answerButtonObject_B;
    [SerializeField] private GameObject answerButtonObject_F1;
    [SerializeField] private GameObject answerButtonObject_F2;
    [SerializeField] private GameObject answerButtonObject_F3;

    [SerializeField] private GameObject messegerHomeCanvas;

    [SerializeField] private GameObject messengerCanvas_Mother;
    [SerializeField] private GameObject messengerCanvas_Father;
    [SerializeField] private GameObject messengerCanvas_Brother;
    [SerializeField] private GameObject messengerCanvas_Friend1;
    [SerializeField] private GameObject messengerCanvas_Friend2;
    [SerializeField] private GameObject messengerCanvas_Friend3;

    [SerializeField] private Button ToggleButton_M;
    [SerializeField] private Button ToggleButton_F;
    [SerializeField] private Button ToggleButton_B;
    [SerializeField] private Button ToggleButton_F1;
    [SerializeField] private Button ToggleButton_F2;
    [SerializeField] private Button ToggleButton_F3;

    private void Update() 
    {
        CheckButtonState(answerButtonObject_M, ToggleButton_M);
        CheckButtonState(answerButtonObject_F, ToggleButton_F);
        CheckButtonState(answerButtonObject_B, ToggleButton_B);
        CheckButtonState(answerButtonObject_F1, ToggleButton_F1);
        CheckButtonState(answerButtonObject_F2, ToggleButton_F2);
        CheckButtonState(answerButtonObject_F3, ToggleButton_F3);
    }

    private void CheckButtonState(GameObject answerButton, Button toggleButton)
    {
        if (answerButton == null || toggleButton == null) return;

        if (!answerButton.activeSelf)
        {
            toggleButton.interactable = TextsController.instance?.GetIsEnd() == true;
        }
        else
        {
            toggleButton.interactable = true;
        }
    }

    public void ActivateMessengerHomeUI()
    {
        if (messegerHomeCanvas == null)
        {
            Debug.LogError("Messenger Prefab 또는 Messenger Canvas가 Inspector에서 설정되지 않았습니다.");
            return;
        }

        if (!messegerHomeCanvas.activeSelf)
        {
            messegerHomeCanvas.SetActive(true);
        }
    }

    public void ActivateMessengerUI_Mother()
    {
        ActivateMessengerUI(messengerCanvas_Mother);
    }

    public void ActivateMessengerUI_Father()
    {
        ActivateMessengerUI(messengerCanvas_Father);
    }

    public void ActivateMessengerUI_Brother()
    {
        ActivateMessengerUI(messengerCanvas_Brother);
    }

    public void ActivateMessengerUI_Friend1()
    {
        ActivateMessengerUI(messengerCanvas_Friend1);
    }

    public void ActivateMessengerUI_Friend2()
    {
        ActivateMessengerUI(messengerCanvas_Friend2);
    }

    public void ActivateMessengerUI_Friend3()
    {
        ActivateMessengerUI(messengerCanvas_Friend3);
    }

    private void ActivateMessengerUI(GameObject messengerCanvas)
    {
        if (messengerCanvas == null)
        {
            Debug.LogError("Messenger Canvas가 할당되지 않았거나 삭제되었습니다.");
            return;
        }

        if (!messengerCanvas.activeSelf)
        {
            if (messegerHomeCanvas != null)
            {
                messegerHomeCanvas.SetActive(false);
            }
            messengerCanvas.SetActive(true);
        }
    }

    public void ToggleMessengerCanvas()
    {
        if (messegerHomeCanvas != null)
        {
            messegerHomeCanvas.SetActive(false);
        }

        ToggleCanvas(messengerCanvas_Mother);
        ToggleCanvas(messengerCanvas_Father);
        ToggleCanvas(messengerCanvas_Brother);
        ToggleCanvas(messengerCanvas_Friend1);
        ToggleCanvas(messengerCanvas_Friend2);
        ToggleCanvas(messengerCanvas_Friend3);
    }

    private void ToggleCanvas(GameObject messengerCanvas)
    {
        if (messengerCanvas != null)
        {
            Canvas canvasComponent = messengerCanvas.GetComponent<Canvas>();
            if (canvasComponent != null)
            {
                canvasComponent.enabled = false;
            }
            else
            {
                messengerCanvas.SetActive(false);
            }
        }
    }
}
