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

    [Header("타 캐릭터들에 대한 초기 의심도 설정")]
    public List<SuspicionEntry> initialSuspicionList = new List<SuspicionEntry>();

    // 런타임에서 사용할 딕셔너리 (대상 캐릭터 -> 의심도 수치)
    public Dictionary<InteractionObjectType, int> suspicionDict { get; private set; } = new Dictionary<InteractionObjectType, int>();

    // 인스펙터 설정을 딕셔너리로 변환 및 누락된 항목 자동 채우기
    public void InitSuspicionContainer()
    {
        suspicionDict.Clear();

        // 1. 인스펙터에 직접 설정한 대상과 의심도 값을 딕셔너리에 먼저 주입
        foreach (var entry in initialSuspicionList)
        {
            if (!suspicionDict.ContainsKey(entry.targetType))
            {
                suspicionDict.Add(entry.targetType, entry.suspicionLevel);
            }
        }

        // 2. 방어 코드: Enum에 존재하지만 인스펙터에서 누락된 대상이 있다면 의심도 0으로 자동 추가 (KeyNotFoundException 방지)
        foreach (InteractionObjectType type in Enum.GetValues(typeof(InteractionObjectType)))
        {
            if (!suspicionDict.ContainsKey(type))
            {
                suspicionDict.Add(type, 0);
            }
        }
    }
}

public class CharacterStatusManager : Singleton<CharacterStatusManager>
{
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
        
        // 각 캐릭터의 상태 데이터 등록 및 내부 의심도 딕셔너리 초기화
        foreach (var status in initialStatuses)
        {
            if (!statusDict.ContainsKey(status.characterType))
            {
                status.InitSuspicionContainer();
                statusDict.Add(status.characterType, status);
            }
            else
            {
                Debug.LogWarning($"[CharacterStatusManager] 중복된 캐릭터 상태가 인스펙터에 설정되었습니다: {status.characterType}");
            }
        }

        // 방어 코드: 매니저 인스펙터 자체에서 누락된 캐릭터가 있다면 기본값으로 자동 생성
        foreach (InteractionObjectType type in Enum.GetValues(typeof(InteractionObjectType)))
        {
            if (!statusDict.ContainsKey(type))
            {
                CharacterStatus defaultStatus = new CharacterStatus { characterType = type };
                defaultStatus.InitSuspicionContainer();
                statusDict.Add(type, defaultStatus);
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
        Debug.LogError($"[CharacterStatusManager] {type}의 상태 데이터를 찾을 수 없습니다.");
        return null;
    }

    // ==========================================
    // 외부 스크립트에서 참조할 데이터 접근 및 제어 메서드들
    // ==========================================

    // 1. 생존 여부
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

    // 2. 감염 여부
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

    // 3. 의심도 가져오기 (주체 캐릭터가 대상 캐릭터를 얼마나 의심하는지)
    public int GetSuspicion(InteractionObjectType subject, InteractionObjectType target)
    {
        var status = GetCharacterStatus(subject);
        if (status != null && status.suspicionDict.TryGetValue(target, out int level))
        {
            return level;
        }
        return 0;
    }

    // 4. 의심도 증감시키기 (주체 캐릭터가 대상 캐릭터를 의심하는 수치를 변동)
    public void AddSuspicion(InteractionObjectType subject, InteractionObjectType target, int amount)
    {
        var status = GetCharacterStatus(subject);
        if (status != null)
        {
            if (status.suspicionDict.ContainsKey(target))
            {
                status.suspicionDict[target] += amount;
                status.suspicionDict[target] = Mathf.Max(0, status.suspicionDict[target]); // 음수 방지 안전장치
            }
        }
    }

    // 5. 특정 대상을 투표할 기준치를 넘었는지 확인 (주체 캐릭터가 대상 캐릭터를 투표해야 하는가?)
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