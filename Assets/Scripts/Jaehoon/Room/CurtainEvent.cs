using UnityEngine;

public class CurtainEvent : MonoBehaviour
{
    // Animator 컴포넌트를 참조하기 위한 변수
    private Animator animator;

    private void Awake() 
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // Animator가 없을 경우 경고 메시지 출력
        if (animator == null)
        {
            Debug.LogWarning("Animator 컴포넌트를 찾을 수 없습니다. CurtainEvent를 사용하려면 Animator가 필요합니다.");
        }
    }

    void OnMouseDown()
    {
        // Animator가 존재할 경우 isOpen 매개변수를 true로 설정
        if (animator != null)
        {
            animator.SetBool("IsOpen", true);
        }
    }
}
