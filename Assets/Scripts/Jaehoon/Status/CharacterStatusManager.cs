using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SuspicionEntry
{
    public InteractionObjectType targetType; // 의심의 대상이 되는 캐릭터
    public int suspicionLevel = 0;           // 해당 대상에 대한 의심도 수치
}

[System.Serializable]
public class CharacterStatus
{
    [Header("캐릭터 종류")]
    public InteractionObjectType characterType;

    [Header("상태 정보")]
    public bool isAlive = true;       // 생존 여부
    public bool isInfected = false;   // 감염 여부

    [Header("투표 기준치")]
    [Tooltip("어떤 대상에 대한 의심도가 이 값을 넘으면 그 대상을 투표함")]
    public int voteThreshold = 50;    // 투표 스레시홀드

    [Header("초기 의심도 세팅 (게임 시작 전 설정용)")]
    public List<SuspicionEntry> initialSuspicionList = new List<SuspicionEntry>();

    [Header("현재 의심도 (게임 실행 중 실시간 확인용)")]
    public List<SuspicionEntry> currentSuspicionView = new List<SuspicionEntry>();

    // 런타임에서 로직 처리에 사용할 딕셔너리 (대상 캐릭터 -> 의심도 수치)
    public Dictionary<InteractionObjectType, int> suspicionDict { get; private set; } = new Dictionary<InteractionObjectType, int>();

    // 💡 변경: 매개변수로 '실제 캐릭터 목록'을 넘겨받아 그것만 초기화함
    public void InitSuspicionContainer(InteractionObjectType[] validCharacters)
    {
        suspicionDict.Clear();

        // 1. 먼저 실제 캐릭터 목록을 딕셔너리에 0으로 쫙 깔아둠 (None, 사물 등 제외)
        foreach (InteractionObjectType charType in validCharacters)
        {
            suspicionDict.Add(charType, 0);
        }

        // 2. 인스펙터에 개발자가 직접 세팅해 둔 값이 있다면, 그 값으로 덮어씌움
        foreach (var entry in initialSuspicionList)
        {
            // 인스펙터에 엉뚱한 값(사물 등)을 넣었을 수도 있으니 검사 후 덮어쓰기
            if (suspicionDict.ContainsKey(entry.targetType))
            {
                suspicionDict[entry.targetType] = entry.suspicionLevel;
            }
        }

        SyncInspectorView(); // 초기 세팅 후 뷰 동기화
    }

    // 딕셔너리의 현재 값을 인스펙터 뷰 전용 리스트로 복사(동기화)
    public void SyncInspectorView()
    {
        currentSuspicionView.Clear();
        foreach (var kvp in suspicionDict)
        {
            currentSuspicionView.Add(new SuspicionEntry { targetType = kvp.Key, suspicionLevel = kvp.Value });
        }
    }
}

public class CharacterStatusManager : Singleton<CharacterStatusManager>
{
    // 💡 [추가] 의심도의 주체나 대상이 될 수 있는 '진짜 캐릭터' 목록
    // (VoteManager 등 다른 곳에서도 참조할 수 있도록 public static으로 노출해도 좋아!)
    public static readonly InteractionObjectType[] ValidCharacters = {
        InteractionObjectType.Player,
        InteractionObjectType.James,
        InteractionObjectType.Nicholas,
        InteractionObjectType.Ella,
        InteractionObjectType.Sophia
    };

    [Header("캐릭터 초기 상태 설정")]
    [SerializeField] private List<CharacterStatus> initialStatuses = new List<CharacterStatus>();

    // 캐릭터 본인의 ID(Enum)로 해당 캐릭터의 전체 상태 데이터에 접근하기 위한 딕셔너리
    private Dictionary<InteractionObjectType, CharacterStatus> statusDict = new Dictionary<InteractionObjectType, CharacterStatus>();

    protected override void Awake()
    {
        base.Awake();
        InitStatusManager();
    }

    private void InitStatusManager()
    {
        statusDict.Clear();
        
        // 1. 인스펙터에 설정된 데이터를 딕셔너리에 등록 (사람 캐릭터만 필터링)
        foreach (var status in initialStatuses)
        {
            // 만약 인스펙터에 실수로 사물을 캐릭터 상태로 등록해 두었다면 무시
            if (Array.Exists(ValidCharacters, c => c == status.characterType))
            {
                if (!statusDict.ContainsKey(status.characterType))
                {
                    // 초기화할 때 실제 캐릭터 배열을 넘겨줌
                    status.InitSuspicionContainer(ValidCharacters);
                    statusDict.Add(status.characterType, status);
                }
                else
                {
                    Debug.LogWarning($"[CharacterStatusManager] 중복된 캐릭터 상태가 인스펙터에 설정되었습니다: {status.characterType}");
                }
            }
        }

        // 2. 방어 코드: ValidCharacters 중에 인스펙터 설정이 누락된 캐릭터가 있다면 기본값으로 자동 생성
        foreach (InteractionObjectType charType in ValidCharacters)
        {
            if (!statusDict.ContainsKey(charType))
            {
                CharacterStatus defaultStatus = new CharacterStatus { characterType = charType };
                defaultStatus.InitSuspicionContainer(ValidCharacters);
                statusDict.Add(charType, defaultStatus);
            }
        }
    }

    // 특정 캐릭터의 전체 상태 객체를 안전하게 가져오는 내부 메서드
    public CharacterStatus GetCharacterStatus(InteractionObjectType type)
    {
        if (statusDict.TryGetValue(type, out CharacterStatus status))
        {
            return status;
        }
        // 캐릭터가 아닌 사물을 조회하려고 할 때는 경고 없이 null 반환 (불필요한 로그 방지)
        return null;
    }

    // ==========================================
    // 외부 스크립트에서 참조할 데이터 접근 및 제어 메서드들
    // ==========================================

    public bool IsAlive(InteractionObjectType type)
    {
        var status = GetCharacterStatus(type);
        return status != null && status.isAlive;
    }

    public void SetAlive(InteractionObjectType type, bool alive)
    {
        var status = GetCharacterStatus(type);
        if (status != null) status.isAlive = alive;
    }

    public bool IsInfected(InteractionObjectType type)
    {
        var status = GetCharacterStatus(type);
        return status != null && status.isInfected;
    }

    public void SetInfected(InteractionObjectType type, bool infected)
    {
        var status = GetCharacterStatus(type);
        if (status != null) status.isInfected = infected;
    }

    // 의심도 가져오기
    public int GetSuspicion(InteractionObjectType subject, InteractionObjectType target)
    {
        var status = GetCharacterStatus(subject);
        if (status != null && status.suspicionDict.TryGetValue(target, out int level))
        {
            return level;
        }
        return 0;
    }

    // 의심도 증감시키기 (ADD)
    public void AddSuspicion(InteractionObjectType subject, InteractionObjectType target, int amount)
    {
        var status = GetCharacterStatus(subject);
        if (status != null)
        {
            if (status.suspicionDict.ContainsKey(target))
            {
                status.suspicionDict[target] += amount;
                status.suspicionDict[target] = Mathf.Max(0, status.suspicionDict[target]); 
                
                status.SyncInspectorView();
            }
        }
    }

    // 의심도를 특정 수치로 고정하기 (SET)
    public void SetSuspicion(InteractionObjectType subject, InteractionObjectType target, int amount)
    {
        var status = GetCharacterStatus(subject);
        if (status != null)
        {
            if (status.suspicionDict.ContainsKey(target))
            {
                status.suspicionDict[target] = Mathf.Max(0, amount); 
                status.SyncInspectorView();
            }
        }
    }

    // 투표 스레시홀드를 넘겼는지 확인
    public bool CheckShouldVoteAgainstTarget(InteractionObjectType subject, InteractionObjectType target)
    {
        var status = GetCharacterStatus(subject);
        if (status != null && status.suspicionDict.TryGetValue(target, out int level))
        {
            return level >= status.voteThreshold;
        }
        return false;
    }
}