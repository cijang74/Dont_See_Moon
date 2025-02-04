using UnityEngine;
using UnityEngine.InputSystem.OSX;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ScenarioManager : MonoBehaviour
{
    private static ScenarioManager instance = null;

    public bool eventA = false;
    public bool eventB = false;
    public bool eventC = false;
    public bool eventD = false; //특정 매체나 방문자 등등 여러 이벤트들 실행되는지 판별 변수, 해당 변수가 참이 되어야 해당 이벤트 실행

    public List<GameObject> eventList; //여기에 특정 이벤트가 일어났는지 판별하는 스크립트 붙은 오브젝트들 넣기

    void Awake() 
    {
        if(instance == null) //싱글톤 생성용
        {
            instance =this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public static ScenarioManager Instance //싱글톤 생성용
    {
        get
        {
            if(instance == null)
                return null;
            return instance;
        }
    }

    public int playedDay=0; //진행일차 변수

    public List<ScenarioNode> progressingNodeList; //진행되는 시나리오 노드는 이 리스트에 저장됩니다

    void Start()
    {
        
    }

    
    void Update()
    {
        foreach(var temp in progressingNodeList) //진행되는 노드를 모두 순회하며 연산
        {
            Scenarioprogresser(temp);
        }
    }

    void Scenarioprogresser(ScenarioNode node)
    {
        //노드 굴리기+종료 조건 판별 코드 작성부
        if(!node.nodeStartFlag)
        {
            if(IsScenarioOccurence(node.nodeOccurenceDate, ref node.occurenceProbability))
            {
                node.nodeStartFlag = true;
            }
            else
                return; //노드 진행 안되면 스킵
        }

        node.RunScenarioNode(); //노드 내 메서드 실행
        
    }

    public bool IsScenarioOccurence(int nodeOccurenceDate, ref float occurenceProbability) //확률에 따라 해당 노드 진행, 안될 시 확률 가충치 부여
    {
        if(nodeOccurenceDate <= ScenarioManager.Instance.playedDay) //최소 발생 일자 이상이어야 발생 가능 조건 달성
        {
            if(Random.value < occurenceProbability)
            {
                return true; //실행
            }
            else
            {
                occurenceProbability += 0.6f;
                return false; //실패, 가중치 부여
            }
        }
        return false;
    }
}


public class ScenarioNode
{
    public int nodeID; //노드 구분용 ID
    public int nodeOccurenceDate; //노드 최소 발생 일자
    public float occurenceProbability; //노드 발생 확률, 발생 일자에 확정적으로 발생하려면 1로 하시오
    public bool nodeStartFlag; //노드 시작 플래그, 켜지면 해당 노드 진행 연산
    public bool nodeEndFlag; //노드 종료 플래그, 켜지면 해당 노드 종료, 이후 진행될 노드가 있다면 리스트에 넣고 종료
    

    public ScenarioNode(int nodeID, int nodeOccurenceDate, float occurenceProbability)
    {
        this.nodeID = nodeID;
        this.nodeOccurenceDate = nodeOccurenceDate;
        this.occurenceProbability = occurenceProbability;
        nodeStartFlag = false;
        nodeEndFlag = false;
    }

    public void RunScenarioNode() //노드 진행용으로 쓰면 됨니다
    {

    }

}

public class ScenarioNode_1 : ScenarioNode //1번 시나리오 노드
{
    public ScenarioNode_1(int nodeID, int nodeOccurenceDate, float occurenceProbability) : base(nodeID, nodeOccurenceDate, occurenceProbability)
    {

    }

}

public class ScenarioNode_2 : ScenarioNode //2번 시나리오 노드
{
    public ScenarioNode_2(int nodeID, int nodeOccurenceDate, float occurenceProbability) : base(nodeID, nodeOccurenceDate, occurenceProbability)
    {

    }
}