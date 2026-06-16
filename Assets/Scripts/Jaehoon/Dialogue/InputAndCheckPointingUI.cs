using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputAndCheckPointingUI : MonoBehaviour
{
    private List<RaycastResult> uiResults = new List<RaycastResult>();

    public bool IsPointingUI()
    {
        // EventSystem이 없거나 마우스 연결이 안 되어 있으면 무시
        if (EventSystem.current == null || Mouse.current == null) return false;

        // 마우스 왼쪽 버튼을 클릭한 순간에만 UI 레이캐스트 진행
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Mouse.current.position.ReadValue();

            uiResults.Clear();
            EventSystem.current.RaycastAll(eventData, uiResults);

            foreach (RaycastResult result in uiResults)
            {
                // UI 오브젝트가 원하는 태그를 가지고 있는지 확인
                if (result.gameObject.CompareTag("DialogueSceneUI"))
                {
                    return true; 
                }
            }
        }

        return false;
    }
}