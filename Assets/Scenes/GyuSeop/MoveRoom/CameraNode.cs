using UnityEngine;
using Unity.Cinemachine; // 유니티 6 시네머신

public class CameraNode : MonoBehaviour
{
    [Header("이 오브젝트를 클릭했을 때 활성화될 카메라")]
    public CinemachineCamera targetCamera;

    [Header("이 장소로 이동할 때 변경할 BGM (선택사항)")]
    [Tooltip("비워두면 기존 BGM이 그대로 유지됩니다.")]
    public AudioClip newBgmClip;

    [Header("전환 효과음 토글")]
    [Tooltip("체크를 해제하면 이 오브젝트를 클릭해 이동할 때 효과음이 재생되지 않습니다.")]
    public bool useTransitionSFX = true;

    [Header("전환 효과음 커스텀 (선택사항)")]
    [Tooltip("비워두면 내비게이션 매니저의 기본 닫힘 소리가 재생됩니다.")]
    public AudioClip customCloseSFX;
    [Tooltip("비워두면 내비게이션 매니저의 기본 열림 소리가 재생됩니다.")]
    public AudioClip customOpenSFX;
}