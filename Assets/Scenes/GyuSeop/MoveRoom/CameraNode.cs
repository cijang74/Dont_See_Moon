using UnityEngine;
using Unity.Cinemachine; // 유니티 6 시네머신

public class CameraNode : MonoBehaviour
{
    [Header("이 오브젝트를 클릭했을 때 활성화될 카메라")]
    public CinemachineCamera targetCamera;

    [Header("전환 효과음 토글")]
    [Tooltip("체크를 해제하면 이 오브젝트를 클릭해 이동할 때 효과음이 재생되지 않습니다.")]
    public bool useTransitionSFX = true;

    [Header("전환 효과음 커스텀 (선택사항)")]
    [Tooltip("비워두면 내비게이션 매니저의 기본 닫힘 소리가 재생됩니다.")]
    public AudioClip customCloseSFX;
    [Tooltip("비워두면 내비게이션 매니저의 기본 열림 소리가 재생됩니다.")]
    public AudioClip customOpenSFX;

    [Header("이동 방식 설정")]
    public bool isSmoothTransition = false;

    [Header("대화 자동 시작 설정")]
    [Tooltip("이 노드가 클릭 후 대화창을 띄우는 오브젝트인지 체크합니다.")]
    public bool isDialogueObject = false;

    [Tooltip("대화할 캐릭터나 오브젝트의 이름을 적어주세요. (예: James)")]
    public string dialogueTargetName;

    // 투표권이 있고 투표 대상이 될 수 있는 실제 캐릭터 목록
    private InteractionObjectType[] validCharacters = {
        InteractionObjectType.Player,
        InteractionObjectType.James,
        InteractionObjectType.Nicholas,
        InteractionObjectType.Ella,
        InteractionObjectType.Sophia
    };

    public bool CanBeClicked()
    {
        if (DialogueManager.Instance != null)
        {
            if (DialogueManager.Instance.isDialoguePlaying)
            {
                return false;
            }

            if (isDialogueObject && !string.IsNullOrEmpty(dialogueTargetName))
            {
                if (System.Enum.TryParse(dialogueTargetName, true, out InteractionObjectType targetType))
                {
                    if (!DialogueManager.Instance.CanInteract(targetType))
                    {
                        return false; // 이미 대화한 대상이면 클릭 불가
                    }
                }
            }
        }
        return true;
    }

    private void Start()
    {
        if (isDialogueObject && targetCamera != null)
        {
            var settings = targetCamera.GetComponent<CameraSettings>();
            if (settings != null)
            {
                settings.onCameraEnterComplete.AddListener(StartNodeDialogue);
            }
        }
    }

    private void StartNodeDialogue()
    {
        bool isInfected = false;

        if (DialogueManager.Instance != null)
        {
            int currentDay = 1;
            DayTransitionManager dayManager = Object.FindFirstObjectByType<DayTransitionManager>();
            if (dayManager != null)
            {
                currentDay = dayManager.currentDay;
            }

            if (System.Enum.TryParse(dialogueTargetName, true, out InteractionObjectType targetType))
            {
                if(dialogueTargetName == "Vote")
                {
                    if(!WorkManager.Instance.isWorkToday)
                    {
                        isInfected = true;
                    }
                }

                else if(dialogueTargetName == "Work")
                {
                    WorkManager.Instance.isWorkToday = true;
                    StartCoroutine(DialogueManager.Instance.StartDialogue(1, targetType, isInfected));
                    return;
                }

                else 
                {
                    bool isCharacter = System.Array.Exists(validCharacters, element => element == targetType);
                    
                    if (!isCharacter)
                    {
                        EvidenceManager.Instance.AcquireEvidence(targetType);
                    }
                }

                StartCoroutine(DialogueManager.Instance.StartDialogue(currentDay, targetType, isInfected));
            }
            else
            {
                Debug.LogError($"[CameraNode] '{dialogueTargetName}' 이름을 InteractionObjectType Enum으로 변환할 수 없습니다. 오타가 없는지 확인해주세요.");
            }
        }
    }
}