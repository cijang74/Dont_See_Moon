using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("카메라 주시 설정")]
    [Tooltip("체크하면 매 프레임 메인 카메라를 바라봅니다.")]
    public bool alwaysLookAtCamera = true;
    
    [Tooltip("체크하면 Y축으로만 회전하여 캐릭터가 기울어지지 않습니다.")]
    public bool lockYAxis = true;

    [Tooltip("지정된 Transform의 위치와 회전값으로 NPC를 이동시킵니다.")]
    public void MoveToPosition(Transform targetPos)
    {
        if (targetPos != null)
        {
            transform.position = targetPos.position;
            
            if (!alwaysLookAtCamera)
            {
                transform.rotation = targetPos.rotation;
            }
        }
        else
        {
            Debug.LogWarning($"[NPCController] {gameObject.name}의 이동 대상(Target)이 할당되지 않았습니다.");
        }
    }

    void Update()
    {
        if (alwaysLookAtCamera && Camera.main != null)
        {
            Vector3 directionToCamera = Camera.main.transform.position - transform.position;

            if (lockYAxis)
            {
                directionToCamera.y = 0;
            }

            if (directionToCamera != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(directionToCamera);
            }
        }
    }
}
