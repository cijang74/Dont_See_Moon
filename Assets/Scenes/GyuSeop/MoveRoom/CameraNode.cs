using UnityEngine;
using Unity.Cinemachine; // 유니티 6 시네머신

public class CameraNode : MonoBehaviour
{
    [Header("이 오브젝트를 클릭했을 때 활성화될 카메라")]
    public CinemachineCamera targetCamera;

    [Header("이 장소로 이동할 때 변경할 BGM (선택사항)")]
    [Tooltip("비워두면 기존 BGM이 그대로 유지됩니다.")]
    public AudioClip newBgmClip;
}