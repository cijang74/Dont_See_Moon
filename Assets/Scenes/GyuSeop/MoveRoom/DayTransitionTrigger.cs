using UnityEngine;

/// <summary>
/// 특정 오브젝트(침대 등)를 클릭했을 때 DayTransitionManager를 통해 다음 날짜로 넘어가는 연출을 실행하는 스크립트입니다.
/// 이 스크립트를 클릭할 오브젝트에 부착하고, 해당 오브젝트에는 Collider(예: BoxCollider)가 있어야 합니다.
/// </summary>
public class DayTransitionTrigger : MonoBehaviour
{
    [Tooltip("씬에 있는 DayTransitionManager 오브젝트를 연결해주세요.")]
    public DayTransitionManager dayManager;

    private void OnMouseDown()
    {
        // Manager가 연결되어 있는지 확인
        if (dayManager != null)
        {
            // 현재 화면 전환 연출 중이 아닐 때만 실행되도록 중복 방지
            if (!DayTransitionManager.IsTransitioning)
            {
                Debug.Log("[DayTransitionTrigger] 날짜 전환을 시작합니다.");
                dayManager.TriggerTransition();
            }
            else
            {
                Debug.Log("[DayTransitionTrigger] 이미 날짜 전환 연출이 진행 중입니다.");
            }
        }
        else
        {
            Debug.LogWarning("[DayTransitionTrigger] DayTransitionManager가 할당되지 않았습니다! 인스펙터를 확인해주세요.");
        }
    }
}
