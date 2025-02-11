using UnityEngine;
using UnityEngine.InputSystem.OSX;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;

//#####################################################################
//             *****각 시나리오 노드 구현 코드 뭉치들****
//#####################################################################


public class ScenarioNode_1 : ScenarioNode //1번 시나리오 노드
{
    public ScenarioNode_1(int nodeID, int nodeOccurenceDate, float occurenceProbability) : base(nodeID, nodeOccurenceDate, occurenceProbability)
    {
        this.nextScenarioNode = new ScenarioNode_2(1, 5, 0.3f); //2번 시나리오 노드 id 1, 5일 공백, 30%확률로 실행
    }

    public override void RunScenarioNode()
    {
        Debug.Log("1번 시나리오 노드 실행 중");
    }

    public override void FlagChecker() //시나리오 노드 조건 달성 판별
    {
        if(ScenarioManager.Instance.eventA) //eventA가 만족되면 1번 시나리오 노드 종료 조건 달성
        {
            
        }
        
    }
}

public class ScenarioNode_2 : ScenarioNode //2번 시나리오 노드
{
    public ScenarioNode nextScenarioNode2;
    public ScenarioNode_2(int nodeID, int nodeOccurenceDate, float occurenceProbability) : base(nodeID, nodeOccurenceDate, occurenceProbability)
    {
        this.nextScenarioNode = new ScenarioNode_3(2, 2, 0.1f); //3번 시나리오 노드 id 2, 2일 공백, 10%확률로 실행
        this.nextScenarioNode2 = new ScenarioNode_4(3, 2, 0.1f); //4번 시나리오 노드 id 3, 2일 공백, 10%확률로 실행

    }

    public override void RunScenarioNode()
    {
        Debug.Log("2번 시나리오 노드 실행 중");
    }

    public override void FlagChecker() //시나리오 노드 조건 달성 판별
    {
        if(ScenarioManager.Instance.eventB && ScenarioManager.Instance.eventC) //eventB, C 가 만족되면 1번 시나리오 노드 종료 조건 달성
        {
            nodeEndFlag = true;
        }
    }

    public override void NodeInjector(ref List<ScenarioNode> tempNode) //임시 노드 리스트에 다음 노드 추가하기기
    {
        base.NodeInjector(ref tempNode);

        if(nextScenarioNode2 != null)
        {
            nextScenarioNode.MinNodeStartDateSetter(); //다음 노드 minNodeStartDate 값 오늘로 갱신
            tempNode.Add(nextScenarioNode2);
        }
    }
}

public class ScenarioNode_3 : ScenarioNode //3번 시나리오 노드
{
    public ScenarioNode_3(int nodeID, int nodeOccurenceDate, float occurenceProbability) : base(nodeID, nodeOccurenceDate, occurenceProbability)
    {
        this.nextScenarioNode = null;
    }

    public override void RunScenarioNode()
    {
        Debug.Log("3번 시나리오 노드 실행 중");
    }

    public override void FlagChecker() //시나리오 노드 조건 달성 판별
    {
        if(ScenarioManager.Instance.eventB && ScenarioManager.Instance.eventD) //eventB, D 가 만족되면 1번 시나리오 노드 종료 조건 달성
        {
            nodeEndFlag = true;
        }
    }
}

public class ScenarioNode_4 : ScenarioNode //4번 시나리오 노드
{
    public ScenarioNode_4(int nodeID, int nodeOccurenceDate, float occurenceProbability) : base(nodeID, nodeOccurenceDate, occurenceProbability)
    {
        this.nextScenarioNode = null;
    }

    public override void RunScenarioNode()
    {
        Debug.Log("4번 시나리오 노드 실행 중");
    }

    public override void FlagChecker() //시나리오 노드 조건 달성 판별
    {
        if(ScenarioManager.Instance.eventB && ScenarioManager.Instance.eventD) //eventB, D 가 만족되면 1번 시나리오 노드 종료 조건 달성
        {
            nodeEndFlag = true;
        }
    }
}