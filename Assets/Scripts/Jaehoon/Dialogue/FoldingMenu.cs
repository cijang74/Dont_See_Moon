using UnityEngine;
using UnityEngine.UI;

public class FoldingMenu : MonoBehaviour
{
    [SerializeField] GameObject hiddenContentPanel;
    [SerializeField] Image foldButtonImage;

    // 현재 메뉴가 열려있는지 확인하는 변수
    private bool isExpanded = false; 

    // 버튼을 누를 때마다 실행될 함수
    public void ToggleMenu()
    {
        // 상태를 반대로 뒤집음 (true -> false, false -> true)
        isExpanded = !isExpanded;

        // 패널을 켜거나 끔
        hiddenContentPanel.SetActive(isExpanded);

        // 상태에 따라 화살표 이미지 회전
        foldButtonImage.transform.localRotation = isExpanded 
            ? Quaternion.Euler(0, 0, 270) 
            : Quaternion.Euler(0, 0, 180);
    }
}
