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
    public UnityEvent onCameraEnter;
    public UnityEvent onCameraExit;
}