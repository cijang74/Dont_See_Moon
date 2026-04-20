using UnityEngine;
using Unity.Cinemachine; // 유니티 6 시네머신

public class CameraNode : MonoBehaviour
{
    [Header("이 오브젝트를 클릭했을 때 활성화될 카메라")]
    public CinemachineCamera targetCamera;
}