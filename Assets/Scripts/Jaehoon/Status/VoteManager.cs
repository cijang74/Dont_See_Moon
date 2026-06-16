using System;
using System.Collections.Generic;
using UnityEngine;

public class VoteManager : Singleton<VoteManager>
{
    // 플레이어가 오늘 투표한 대상 (매일 초기화됨)
    private InteractionObjectType playerCurrentVote = InteractionObjectType.None;

    // 투표권이 있고 투표 대상이 될 수 있는 실제 캐릭터 목록
    private InteractionObjectType[] validCharacters = {
        InteractionObjectType.Player,
        InteractionObjectType.James,
        InteractionObjectType.Nicholas,
        InteractionObjectType.Ella,
        InteractionObjectType.Sophia
    };

    // 1️⃣ DialogueSystem에서 투표 버튼을 눌렀을 때 호출되는 메서드
    public void SavePlayerVote(string votedCharacterName)
    {
        if (Enum.TryParse(votedCharacterName, out InteractionObjectType target))
        {
            playerCurrentVote = target;
            Debug.Log($"[VoteManager] 플레이어가 {target}에게 투표를 완료했습니다.");
        }
    }

    // 2️⃣ DayTransitionManager에서 날짜가 넘어갈 때(화면이 까매졌을 때) 호출되는 정산 메서드
    public void CalculateDailyVoteResults()
    {
        Debug.Log("========== [VoteManager] 일일 투표 정산 시작 ==========");

        Dictionary<InteractionObjectType, int> voteCounts = new Dictionary<InteractionObjectType, int>();
        int aliveCount = 0;

        // [STEP 1] 살아있는 캐릭터 파악 및 득표수 0으로 초기화
        foreach (InteractionObjectType charType in validCharacters)
        {
            if (CharacterStatusManager.Instance.IsAlive(charType))
            {
                aliveCount++;
                voteCounts[charType] = 0; 
            }
        }

        // [STEP 2] 플레이어의 투표 반영
        if (playerCurrentVote != InteractionObjectType.None && voteCounts.ContainsKey(playerCurrentVote))
        {
            voteCounts[playerCurrentVote]++;
            Debug.Log($"[VoteManager] Player -> {playerCurrentVote} (1표 행사)");
        }

        // [STEP 3] NPC들의 투표 반영 (스레시홀드를 넘긴 대상 중 의심도가 가장 높은 사람에게 1표)
        foreach (InteractionObjectType voter in validCharacters)
        {
            // 플레이어는 위에서 이미 처리했고, 죽은 NPC는 투표권이 없음
            if (voter == InteractionObjectType.Player || !CharacterStatusManager.Instance.IsAlive(voter)) 
                continue;

            InteractionObjectType targetToVote = InteractionObjectType.None;
            int maxSuspicion = -1;

            foreach (InteractionObjectType target in validCharacters)
            {
                // 죽은 사람에게는 투표하지 않음, 자기 자신에게도 투표 불가 (필요시 제거 가능)
                if (voter == target || !CharacterStatusManager.Instance.IsAlive(target)) 
                    continue;

                // 스레시홀드를 넘겼는지 확인
                if (CharacterStatusManager.Instance.CheckShouldVoteAgainstTarget(voter, target))
                {
                    int suspicion = CharacterStatusManager.Instance.GetSuspicion(voter, target);
                    
                    // 스레시홀드를 넘긴 대상 중 가장 의심도가 높은 사람 갱신
                    if (suspicion > maxSuspicion)
                    {
                        maxSuspicion = suspicion;
                        targetToVote = target;
                    }
                }
            }

            // 투표할 대상을 찾았다면 표 추가
            if (targetToVote != InteractionObjectType.None)
            {
                voteCounts[targetToVote]++;
                Debug.Log($"[VoteManager] {voter} -> {targetToVote} (의심도: {maxSuspicion} / 1표 행사)");
            }
        }

        // [STEP 4] 득표 결과 집계
        InteractionObjectType maxVotedCharacter = InteractionObjectType.None;
        int maxVotes = 0;
        bool isTie = false;

        foreach (var kvp in voteCounts)
        {
            if (kvp.Value > maxVotes)
            {
                maxVotes = kvp.Value;
                maxVotedCharacter = kvp.Key;
                isTie = false;
            }
            else if (kvp.Value == maxVotes && maxVotes > 0)
            {
                isTie = true; // 동률 발생
            }
        }

        // [STEP 5] 과반수 체크 및 처형 (소수점 버림 + 1. 예: 5명이면 3표, 4명이면 3표, 3명이면 2표)
        int majorityThreshold = Mathf.FloorToInt(aliveCount / 2f) + 1;
        Debug.Log($"[VoteManager] 총 생존자: {aliveCount}명 / 과반수 처형 기준: {majorityThreshold}표");

        if (!isTie && maxVotedCharacter != InteractionObjectType.None && maxVotes >= majorityThreshold)
        {
            Debug.Log($"[VoteManager] 💀 {maxVotedCharacter} 캐릭터가 {maxVotes}표로 과반수를 넘겨 처형당합니다!");
            CharacterStatusManager.Instance.SetAlive(maxVotedCharacter, false);
            // TODO: 여기서 누구 죽었다고 팝업을 띄우거나, 연출을 넣을 수 있음
        }
        else
        {
            Debug.Log($"[VoteManager] 🕊️ 투표가 동률이거나 과반수를 넘긴 캐릭터가 없어 아무도 처형되지 않았습니다. (최다 득표: {maxVotes}표)");
        }

        // [STEP 6] 다음 날을 위해 플레이어 투표 초기화
        playerCurrentVote = InteractionObjectType.None;
        Debug.Log("========== [VoteManager] 일일 투표 정산 종료 ==========");
    }
}