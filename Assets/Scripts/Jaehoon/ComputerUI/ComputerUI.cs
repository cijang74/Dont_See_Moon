using UnityEngine;
using UnityEngine.UI;

public class ComputerUI : MonoBehaviour
{
    [SerializeField] private GameObject messegerHomeCanvas; // 메신저 홈 화면 캔버스
    [SerializeField] private GameObject newsHomeCanvas; // 뉴스 홈 화면 캔버스

    [SerializeField] private GameObject[] messengerCanvases; // 개별 메신저 대화 캔버스 배열
    [SerializeField] private GameObject[] answerButtonObjects; // 답변 버튼 배열
    [SerializeField] private Button[] toggleButtons; // 메시지 창을 닫는 버튼 배열

    [SerializeField] private GameObject newsCanvas; // 뉴스 창 캔버스

    private void Update()
    {
        // 모든 버튼의 상태를 자동으로 업데이트 (활성화 여부 및 상호작용 가능 여부)
        for (int i = 0; i < answerButtonObjects.Length; i++)
        {
            if (answerButtonObjects[i] == null || toggleButtons[i] == null) continue;

            // 답변 버튼이 활성화되어 있거나, 대화가 끝났다면 버튼을 활성화
            toggleButtons[i].interactable = answerButtonObjects[i].activeSelf || TextsController.instance?.GetIsEnd() == true;
        }
    }

    // 메신저 홈 UI 활성화
    public void ActivateMessengerHomeUI()
    {
        if (messegerHomeCanvas == null)
        {
            Debug.LogError("Messenger Canvas가 할당되지 않았습니다.");
            return;
        }

        messegerHomeCanvas.SetActive(true); // 메신저 홈 화면을 활성화
    }

    // 뉴스 홈 UI 활성화
    public void ActivateNewsHomeUI()
    {
        if (newsHomeCanvas == null)
        {
            Debug.LogError("News Canvas가 할당되지 않았습니다.");
            return;
        }

        newsHomeCanvas.SetActive(true); // 뉴스 홈 화면을 활성화
    }

    // 특정 인덱스의 메신저 UI 활성화
    public void ActivateMessengerUI(int index)
    {
        // 인덱스가 유효한지 검사하고, 할당되지 않은 경우 오류 메시지 출력
        if (index < 0 || index >= messengerCanvases.Length || messengerCanvases[index] == null)
        {
            Debug.LogError("Messenger Canvas가 할당되지 않았거나 인덱스가 잘못되었습니다.");
            return;
        }

        messegerHomeCanvas?.SetActive(false); // 메신저 홈 화면 비활성화
        messengerCanvases[index].SetActive(true); // 해당 인덱스의 메신저 대화창 활성화
    }

    public void ActivateNewsUI()
    {
        newsHomeCanvas?.SetActive(false); // 뉴스 홈 화면 비활성화
        newsCanvas.SetActive(true); // 뉴스 창 활성화
    }

    // 모든 메신저 캔버스를 비활성화 (메신저 닫기 기능)
    public void ToggleMessengerCanvas()
    {
        messegerHomeCanvas?.SetActive(false); // 메세저 홈 화면 비활성화
        newsHomeCanvas?.SetActive(false); // 뉴스 홈 화면을 활성화
        newsCanvas?.SetActive(false); // 뉴스 홈 화면을 활성화

        foreach (var canvas in messengerCanvases)
        {
            if (canvas == null) continue;

            var canvasComponent = canvas.GetComponent<Canvas>();
            if (canvasComponent != null)
            {
                canvasComponent.enabled = false; // Canvas 컴포넌트가 존재하면 비활성화
            }
            else
            {
                canvas.SetActive(false); // Canvas 컴포넌트가 없다면 오브젝트 자체를 비활성화
            }
        }
    }
}
