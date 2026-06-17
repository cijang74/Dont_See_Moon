using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingManager : Singleton<EndingManager>
{
    // 💡 [추가] 엔딩 조건이 달성되었는지 검사하고 엔딩을 실행하는 메서드
    public bool CheckAndTriggerEnding(int currentDay)
    {
        Debug.Log("currentDay: " + currentDay);
        
        // 1. 플레이어가 사망한 상태라면 -> 배드 엔딩
        if (!CharacterStatusManager.Instance.IsAlive(InteractionObjectType.Player))
        {
            Debug.Log("[EndingManager] 플레이어가 사망했습니다. 배드 엔딩을 시작합니다.");
            StartBadEnding();
            return true; // 엔딩 실행됨
        }

        // TODO: 추후 여기에 다른 엔딩 조건들을 추가하시면 됩니다.
        // 예시 1) 8일차가 되었는데 살아남았다면 노말 엔딩
        if (currentDay >= 8) 
        { 
            bool isSophiaAlive = CharacterStatusManager.Instance.IsAlive(InteractionObjectType.Sophia);

            if(!isSophiaAlive)
            {
                StartNormalEnding();
            }

            if(isSophiaAlive)
            {
                StartAllKillEnding();
            }

            return true; 
        }

        return false; // 어떤 엔딩 조건도 만족하지 않음 (게임 계속 진행)
    }

    public void StartNormalEnding()
    {
        StartCoroutine(EndingRoutine(8));
    }

    public void StartBadEnding()
    {
        StartCoroutine(EndingRoutine(9));
    }

    public void StartAllKillEnding()
    {
        StartCoroutine(EndingRoutine(10));
    }

    private IEnumerator EndingRoutine(int day)
    {
        SceneManager.LoadScene("Scene_Ending");
        yield return null;
        yield return StartCoroutine(DialogueManager.Instance.StartDialogue(day, InteractionObjectType.Ending, false));
        SceneManager.LoadScene("Scene_Start");
    }
}