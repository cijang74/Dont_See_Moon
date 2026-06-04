using UnityEngine;

public class CursorObjectDetection : MonoBehaviour
{
    Outline outline;
    Color onOutlineColor = new Color(1f, 0.5379274f, 0f, 1f);
    Color offOutlineColor = new Color(1f, 0.5379274f, 0f, 0f);

    private void Awake() 
    {
        outline = GetComponent<Outline>();
        outline.OutlineColor = offOutlineColor;
    }

    // 마우스 커서가 이 콜라이더 위에 올라왔을 때 자동으로 한 번 실행
    void OnMouseEnter()
    {
        outline.OutlineColor = onOutlineColor;
    }

    // 마우스 커서가 이 콜라이더를 벗어나거나, 다른 콜라이더에 가려졌을 때 자동으로 한 번 실행
    void OnMouseExit()
    {
        outline.OutlineColor = offOutlineColor;
    }
}
/*
public class CursorObjectDetection : MonoBehaviour
{
    Outline outline;
    Color onOutlineColor = new Color(1f, 0.5379274f, 0f, 1f);
    Color offOutlineColor = new Color(1f, 0.5379274f, 0f, 0f);

    private void Awake() 
    {
        outline = gameObject.GetComponent<Outline>();
    }

    void Update()
    {
        // 카메라에서 마우스 포인터 위치로 광선을 발사합니다.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 광선이 객체에 닿았는지 확인합니다.
        if (Physics.Raycast(ray, out hit))
        {
            // 객체를 가리키고 있는 경우, 객체 이름을 콘솔에 출력합니다.
            if (hit.collider.gameObject == this.gameObject)
            {
                //Debug.Log("커서가 이 객체를 가리키고 있습니다: " + hit.collider.gameObject.name);
                outline.OutlineColor = onOutlineColor;
            }
        }
        
        else
        {
            // 객체를 가리키고 있지 않은 경우 메시지를 출력합니다.
            //Debug.Log("커서가 객체를 가리키고 있지 않습니다.");
            outline.OutlineColor = offOutlineColor;
        }
    }
}
*/