using UnityEngine;
using UnityEngine.Events;

public class CameraSettings : MonoBehaviour
{
    [Header("카메라 고유 설정")]
    [Tooltip("카메라의 계층 (0: 최상위(방 전체), 1: 중간(책상), 2: 하위(서랍) 등)")]
    public int hierarchyLevel = 0;
    
    [Tooltip("이 카메라가 활성화될 때 재생할 BGM (비워두면 기존 BGM 유지)")]
    public AudioClip bgmClip;

    [Header("카메라 이벤트")]
    [Tooltip("카메라 전환이 시작될 때 호출됩니다.")]
    public UnityEvent onCameraEnter;
    [Tooltip("카메라 전환이 완전히 끝났을 때 호출됩니다.")]
    public UnityEvent onCameraEnterComplete;
    
    [Tooltip("이 카메라에서 다른 카메라로 전환이 시작될 때 호출됩니다.")]
    public UnityEvent onCameraExit;
    [Tooltip("다른 카메라로의 전환이 완전히 끝났을 때 호출됩니다.")]
    public UnityEvent onCameraExitComplete;
}