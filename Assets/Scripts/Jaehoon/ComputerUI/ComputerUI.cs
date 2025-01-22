using UnityEngine;

public class ComputerUI : MonoBehaviour
{
    public GameObject messengerPrefab; // Messenger Prefab 오브젝트
    public GameObject messengerCanvas; // Messenger Canvas 오브젝트 직접 참조

    public void ActivateMessengerUI()
    {
        Debug.Log("버튼 눌림");

        if (messengerPrefab == null || messengerCanvas == null)
        {
            Debug.LogError("Messenger Prefab 또는 Messenger Canvas가 Inspector에서 설정되지 않았습니다.");
            return;
        }

        // Messenger Prefab 활성화
        if (!messengerPrefab.activeSelf)
        {
            messengerPrefab.SetActive(true);
        }

        // Messenger Canvas 활성화
        messengerCanvas.SetActive(true);
    }

    public void ToggleMessengerPrefab()
    {
        if (messengerPrefab == null)
        {
            Debug.LogError("Inspector에서 Messenger Prefab이 할당되지 않았습니다.");
            return;
        }

        // Messenger Prefab 활성화/비활성화 토글
        messengerPrefab.SetActive(!messengerPrefab.activeSelf);
    }
}