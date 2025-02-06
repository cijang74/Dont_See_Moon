using UnityEngine;
using UnityEngine.InputSystem.OSX;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;

public class ScenarioManager : MonoBehaviour
{
    private static ScenarioManager instance = null;

    public bool eventA = false;
    public bool eventB = false;
    public bool eventC = false;
    public bool eventD = false; //특정 매체나 방문자 등등 여러 이벤트들 실행되는지 판별 변수, 해당 변수가 참이 되어야 해당 이벤트 실행

    public List<GameObject> eventList = new List<GameObject>(); //여기에 특정 이벤트가 일어났는지 판별하는 스크립트 붙은 오브젝트들 넣기

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

    public List<ScenarioNode> progressingNodeList = new List<ScenarioNode>(); //진행되는 시나리오 노드는 이 리스트에 저장됩니다

    void Start()
    {
        progressingNodeList.Add(new ScenarioNode_1(0, 0, 1f)); //1번 시나리오 노드를 루트 노드로 시작

        foreach(var temp in progressingNodeList) //진행되는 노드를 모두 순회하며 연산
        {
            Scenarioprogresser(temp); //날짜가 바뀐 직후 노드들 순회하며 실행 가챠 시작
        }
    }

    
    void Update()
    {
        foreach(var temp in progressingNodeList) //진행되는 노드를 모두 순회하며 연산, 노드의 메서드 업데이트
        {
            if(temp.nodeStartFlag)
                temp.RunScenarioNode(); //노드 내 메서드 실행
        }
    }

    public void DayChanger() //날짜 바꿀 때마다 한번 씩 실행
    {
        List<ScenarioNode> tempProgressingNodeList = new List<ScenarioNode>();

        foreach(var temp in progressingNodeList) //진행되는 노드를 모두 순회하며 연산
        {
            if(temp.nodeEndFlag) //종료 예정인 노드들의 다음 노드 임시 리스트에 삽입
                tempProgressingNodeList.Add(temp.NodeEjector());
        }

        progressingNodeList.AddRange(tempProgressingNodeList); //새로 추가되는 노드들 실행 리스트에 합치기, 순회 중 노드를 리스트에 넣으면 오버플로우 터짐

        progressingNodeList.RemoveAll(node => node.nodeEndFlag); //nodeEndFlag 가 참인 모든 노드 삭제

        playedDay++; //날짜 올리고

        foreach(var temp in progressingNodeList) //진행되는 노드를 모두 순회하며 연산
        {
            Scenarioprogresser(temp); //날짜가 바뀐 직후 노드들 순회하며 실행 가챠 시작
        }
    }

    void Scenarioprogresser(ScenarioNode node)
    {
        //노드 굴리기+종료 조건 판별 코드 작성부
        if(!node.nodeStartFlag)
        {
            if(IsScenarioOccurence(node.minNodeStartDate, ref node.occurenceProbability))
            {
                node.nodeStartFlag = true;
            }
            else
                return; //노드 진행 안되면 스킵
        }       
    }

    public bool IsScenarioOccurence(int minNodeStartDate, ref float occurenceProbability) //확률에 따라 해당 노드 진행, 안될 시 확률 가충치 부여
    {
        if(minNodeStartDate <= playedDay) //최소 발생 일자 이상이어야 발생 가능 조건 달성
        {
            if(Random.value < occurenceProbability)
            {
                return true; //실행
            }
            else
            {
                occurenceProbability += 0.2f;
                return false; //실패, 가중치 부여 현재값은 20%씩 증가
            }
        }
        return false;
    }
}


public class ScenarioNode //시나리오 노드 기본 클래스
{
    public int nodeID; //노드 구분용 ID
    public int nodeOccurenceDate; //노드 최소 발생 일자, 노드가 progressingNodeList에 올라온 날짜로부터 n일 이후부터 노드 확률적 실행 가능, 이전 노드와 최소한 n일의 공백 발생
    public float occurenceProbability; //노드 발생 확률, 발생 일자에 확정적으로 발생하려면 1로 하시오
    public bool nodeStartFlag; //노드 시작 플래그, 켜지면 해당 노드 진행 연산
    public bool nodeEndFlag; //노드 종료 플래그, 켜지면 해당 노드 종료, 이후 진행될 노드가 있다면 리스트에 넣고 종료, ***이 플래그가 켜지면 실행 노드 리스트 순회 후 바로 노드가 지워지니 주의할 것***
    public int minNodeStartDate; //최소 실행 일자
    //public ScenarioNode nextScenarioNode; //연계될 다음 노드, 만약 조건에 따라 복수의 노드 중 하나가 선택된다면 오버라이딩하면서 더 추가하시오
    

    public ScenarioNode(int nodeID, int nodeOccurenceDate, float occurenceProbability)
    {
        this.nodeID = nodeID;
        this.nodeOccurenceDate = nodeOccurenceDate;
        this.occurenceProbability = occurenceProbability;
        nodeStartFlag = false;
        nodeEndFlag = false;
        minNodeStartDate = ScenarioManager.Instance.playedDay + nodeOccurenceDate; //노드에 삽입되는 날 기준으로부터 노드 최소 발생 일자가 더해진 날짜를 저장
    }

    public virtual void RunScenarioNode() //노드 진행용으로 쓰면 됨니다
    {

    }

    public virtual void FlagChecker()
    {

    }

    public virtual ScenarioNode NodeEjector() //다음 노드 리스트에 올리기
    {
        return null;
    }

}

public class ScenarioNode_1 : ScenarioNode //1번 시나리오 노드
{
    public ScenarioNode_1(int nodeID, int nodeOccurenceDate, float occurenceProbability) : base(nodeID, nodeOccurenceDate, occurenceProbability)
    {

    }

    public override void RunScenarioNode()
    {
        Debug.Log("1번 시나리오 노드 실행 중중중");
        if(!nodeEndFlag)
            FlagChecker();
    }

    public override void FlagChecker() //시나리오 노드 조건 달성 판별
    {
        if(ScenarioManager.Instance.eventA) //eventA가 만족되면 1번 시나리오 노드 종료 조건 달성
        {
            nodeEndFlag = true;
        }
        
    }

    public override ScenarioNode NodeEjector()
    {
        return new ScenarioNode_2(1, 5, 0.3f); //2번 시나리오 노드 id 1, 5일 공백, 30%확률로 실행
    }

}

public class ScenarioNode_2 : ScenarioNode //2번 시나리오 노드
{
    public ScenarioNode_2(int nodeID, int nodeOccurenceDate, float occurenceProbability) : base(nodeID, nodeOccurenceDate, occurenceProbability)
    {

    }

    public override void RunScenarioNode()
    {
        Debug.Log("2번 시나리오 노드 실행 중중중");
        if(!nodeEndFlag)
            FlagChecker();
    }

    public override void FlagChecker() //시나리오 노드 조건 달성 판별
    {
        if(ScenarioManager.Instance.eventB && ScenarioManager.Instance.eventC) //eventB, C 가 만족되면 1번 시나리오 노드 종료 조건 달성
        {
            nodeEndFlag = true;
        }
    }

    public override ScenarioNode NodeEjector()
    {
        return new ScenarioNode_3(2, 2, 0.1f); //3번 시나리오 노드 id 2, 2일 공백, 10%확률로 실행
    }
}

public class ScenarioNode_3 : ScenarioNode //3번 시나리오 노드
{
    public ScenarioNode_3(int nodeID, int nodeOccurenceDate, float occurenceProbability) : base(nodeID, nodeOccurenceDate, occurenceProbability)
    {
        //this.nextScenarioNode = new ScenarioNode_1(0, 1, 0.8f); //1번 시나리오 노드 id 0, 1일 공백, 80%확률로 실행
    }

    public override void RunScenarioNode()
    {
        Debug.Log("3번 시나리오 노드 실행 중중중");
        if(!nodeEndFlag)
            FlagChecker();
    }

    public override void FlagChecker() //시나리오 노드 조건 달성 판별
    {
        if(ScenarioManager.Instance.eventB && ScenarioManager.Instance.eventD) //eventB, D 가 만족되면 1번 시나리오 노드 종료 조건 달성
        {
            nodeEndFlag = true;
        }
    }

    public override ScenarioNode NodeEjector()
    {
        return null;
    }
}