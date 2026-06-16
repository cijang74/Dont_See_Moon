using System.Collections.Generic;
using UnityEngine;

// 상호작용 및 증거로 사용될 오브젝트 타입에 접근하기 위해 기존 Enum 활용
public class EvidenceManager : Singleton<EvidenceManager>
{
    // 획득한 증거들을 저장하는 해시세트 (중복 획득 방지 및 빠른 검색용)
    private HashSet<InteractionObjectType> acquiredEvidences = new HashSet<InteractionObjectType>();

    // 맵에서 증거를 조사/획득했을 때 이 함수를 호출해 줘
    public void AcquireEvidence(InteractionObjectType evidence)
    {
        acquiredEvidences.Add(evidence);
        Debug.Log($"[EvidenceManager] 증거 획득 기록됨: {evidence}");
    }

    // 대화 선택지 등에서 특정 증거를 가지고 있는지 확인할 때 사용
    public bool HasEvidence(InteractionObjectType evidence)
    {
        return acquiredEvidences.Contains(evidence);
    }
}