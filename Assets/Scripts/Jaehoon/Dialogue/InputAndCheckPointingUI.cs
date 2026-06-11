using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using UnityEngine;

public class InputAndCheckPointingUI : MonoBehaviour
{
    private List<RaycastResult> uiResults = new List<RaycastResult>();

    // 최근 멀티 터치를 구현할 때 사용되는 입력 시스템, 직접 OnEnable에서 활성화 시켜주어야 함.
    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        TouchSimulation.Enable(); // 에디터에서 터치 막히는거 방지
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
        EnhancedTouchSupport.Disable();
    }

    

    public bool IsPointingUI()
    {
        // 터치스크린 입력이 있을 때
        if (Touch.activeTouches.Count > 0)
        {
            // 터치 정보를 가져와서
            Touch primaryTouch = Touch.activeTouches[0];

            // 터치 시작이라면
            if (primaryTouch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                // DialogueSceneUI를 눌렀으면
                if(GetDetactUITag("DialogueSceneUI"))
                {
                    return true;
                }
            }
        }
            return false;
    }

    // 터치하고 있는 UI의 태그가 하단 UI 태그라면
    public bool GetDetactUITag(string tagNameToSearch)
    {
        // 터치 스크린을 누른것이 감지되지 않았으면 return
        if (Touch.activeTouches.Count == 0)
        {
            return false;
        }

        // 웬만하면 하나의 터치만 인식
        Touch primaryTouch = Touch.activeTouches[0];
        Vector2 touchPosition = primaryTouch.screenPosition;

        // 터치 위치 기준으로 포인터 이벤트 데이터 생성
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = touchPosition;

        // 결과물 담을 리스트 생성
        // List<RaycastResult> uiResults = new List<RaycastResult>();
        uiResults.Clear();

        // 레이캐스트 쏘기
        EventSystem.current.RaycastAll(eventData, uiResults);

        foreach (RaycastResult result in uiResults)
        {
            // UI 오브젝트가 원하는 태그를 가지고 있는지 확인
            if (result.gameObject.CompareTag(tagNameToSearch))
            {
                // Debug.Log($"찾은 UI: {result.gameObject.name}");
                return true; 
            }
        }
        return false;
    }
}
