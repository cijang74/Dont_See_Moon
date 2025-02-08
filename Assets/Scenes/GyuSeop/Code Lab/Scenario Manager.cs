using UnityEngine;
using UnityEngine.InputSystem.OSX;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;

public class ScenarioManager : MonoBehaviour
{
    private static ScenarioManager instance = null;

    //임시로 만든 bool 변수들, 나중엔 각 정보 매체 스크립트 안에 있는 변수 참조하거나 이 스크립트에 변수 만들고 이걸 각 정보 매체에서 컨트롤 하게 하던지 해야함, 이 스크립트를 싱글톤으로 일단 해놔서 후자 방법이 더 쉬울지도 모르겠다
    public bool eventA = false;
    public bool eventB = false;
    public bool eventC = false;
    public bool eventD = false; //특정 매체나 방문자 등등 여러 이벤트들 실행되는지 판별 변수, 해당 변수가 참이 되어야 해당 이벤트 실행

    public List<GameObject> eventList = new List<GameObject>(); //여기에 특정 이벤트가 일어났는지 판별하는 스크립트 붙은 오브젝트들 넣기, 위에서 말한 주석에서 전자 방법 쓰려면 여기다가 각 정보 매체들 오브젝트 여기다 집어넣고 참조하면 될듯? 필요없으면 삭제 가능
    public float ScenarioNodeOccurrenceProbabilityWeight; //시나리오 노드 발생 확률 가중치

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



    public int playedDay=0; //진행일차 변수, 다른 매니저 스크립트 안에 있으면 그거 참조하도록 바꾸길 바람

    public List<ScenarioNode> progressingNodeList = new List<ScenarioNode>(); //진행되는 시나리오 노드는 이 리스트에 저장됩니다

    void Start()
    {
        ScenarioNodeOccurrenceProbabilityWeight = 0.2f; //노드 실행 실패 시 확률 20퍼센트포인트씩 증가

        progressingNodeList.Add(new ScenarioNode_1(0, 0, 1f)); //1번 시나리오 노드를 루트 노드로 시작, 테스트용으로 박아놓음, 아마 실구현 할 때는 다른 메서드에 초기 시나리오 노드 삽입 코드 구현해서 실행시키면 될듯(랜덤 돌려서 1일차에 켜지는 노드 다양화)

        foreach(var temp in progressingNodeList) //진행되는 노드를 모두 순회하며 연산, 테스트용
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



    public void DayChanger() //돌리면 하루 올라가면서 노드 정리
    {
        playedDay++;
        NextDayScenarioNodeSetter();
    }

    public void NextDayScenarioNodeSetter() //날짜 바꾸면서 노드 정리하는 메서드, ***날짜 바꾼 다음에 실행하시오, 아니면 지연일 하루씩 땡겨짐***
    {
        ScenarioNodeInserter(); //연계 시나리오 노드 있으면 리스트에 넣기
        progressingNodeList.RemoveAll(node => node.nodeEndFlag); //nodeEndFlag 가 참인 모든 노드 삭제

        foreach(var temp in progressingNodeList) //진행되는 노드를 모두 순회하며 시나리오 노드 실행 여부 판별
        {
            Scenarioprogresser(temp); //날짜가 바뀐 직후 노드들 순회하며 실행 가챠 시작, true되면 해당 날짜부터 실행 시작
        }
    }

    public void ScenarioNodeInserter() //다음 시나리오 노드 리스트에 삽입하는 메서드
    {
        List<ScenarioNode> tempProgressingNodeList = new List<ScenarioNode>(); //추가될 노드 임시 저장용 리스트

        foreach(var temp in progressingNodeList) //진행되는 노드를 모두 순회하며 연산
        {
            if(temp.nodeEndFlag) //종료 예정인 노드들의 다음 노드 임시 리스트에 삽입
            {
                //tempProgressingNodeList.Add(temp.NodeEjector());
                if(temp.NodeEjector() is ScenarioNode tempNextNode) //NodeEjector()에서 null이 아니라 노드가 반환되면면 tempNextNode에 저장 후 add() 실행, gpt가 알려준 문법
                    tempProgressingNodeList.Add(tempNextNode);
            }
        }

        progressingNodeList.AddRange(tempProgressingNodeList); //새로 추가되는 노드들 실행 리스트에 합치기, 순회 중 노드를 리스트에 넣으면 오버플로우 터져서 이렇게 함, 터지는지는 직접 해봐서 앎 ㅇㅇ
    }

    void Scenarioprogresser(ScenarioNode node) //노드 실행 가챠 돌리는 메서드
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
                occurenceProbability += ScenarioNodeOccurrenceProbabilityWeight;
                return false; //실패, 가중치 부여 현재값은 20%씩 증가
            }
        }
        return false;
    }
}


//#####################################################################
//              *****이 아래로는 시나리오 노드 클래스****
//#####################################################################
//노드 ID, 실행 지연일, 실행 확률, 클리어 조건 판별, 해당 노드에서 따로 수행할 기능을 넣으면 알아서 돌아가는 *칭구칭긔*
//클래스 덩치가 너무 크다. 다이어트 할 수 있는 쌈@뽕한 방법이 없을까


public class ScenarioNode //시나리오 노드 기본 클래스
{
    public int nodeID; //노드 구분용 ID ,뭐, 리스트에서 특정 노드 찾을 때 쓰세요
    public int nodeOccurenceDate; //실행 지연일, 노드가 progressingNodeList에 올라온 날짜로부터 n일 이후부터 노드 확률적 실행 가능, 이전 노드와 최소한 n일의 공백 발생
    public float occurenceProbability; //노드 실행 확률, 발생 일자에 확정적으로 발생하려면 1로 하시오
    public bool nodeStartFlag; //노드 시작 플래그, 켜지면 해당 노드 기능 실행
    public bool nodeEndFlag; //노드 종료 플래그, 켜지면 해당 노드 종료, 이후 진행될 노드가 있다면 리스트에 넣고 종료, ***이 플래그가 켜지면 날짜가 바뀐 후 해당 노드는 리스트에서 삭제됩니다***
    public int minNodeStartDate; //최소 실행 일자, 해당 노드의 nodeEndFlag가 TRUE 값이 되면 다음날 노드가 삭제되기 전 playedDay(현재 날짜) + nodeOccurenceDate(노드 최소 발생 일자) 에 실행되도록 날짜값 넣고 리스트에 넣습니다

    public ScenarioNode nextScenarioNode; //연계될 다음 노드, 만약 조건에 따라 복수의 노드 중 하나가 선택된다면 오버라이딩하면서 더 추가하시오
    public int nextScenarioNodeOccurenceDate; //다음 진행 노드 최소 발생 일자
    public float nextScenarioNodeOccurenceProbability; //다음 진행 노드 발생 확률
    

    public ScenarioNode(int nodeID, int nodeOccurenceDate, float occurenceProbability)
    {
        this.nodeID = nodeID;
        this.nodeOccurenceDate = nodeOccurenceDate;
        this.occurenceProbability = occurenceProbability;
        nodeStartFlag = false;
        nodeEndFlag = false;
        minNodeStartDate = ScenarioManager.Instance.playedDay + nodeOccurenceDate; //노드에 삽입되는 날 기준으로부터 노드 최소 발생 일자가 더해진 날짜를 저장, 클래스 생성 후 바로 리스트업할 경우 대비해 일단 값은 초기화 함
    }

    public ScenarioNode(int nodeID, int nodeOccurenceDate, float occurenceProbability, ScenarioNode nextScenarioNode) //다음 시나리오 노드 교체할라면 마지막 인자로 줘도 됨
    {
        this.nodeID = nodeID;
        this.nodeOccurenceDate = nodeOccurenceDate;
        this.occurenceProbability = occurenceProbability;
        nodeStartFlag = false;
        nodeEndFlag = false;
        minNodeStartDate = ScenarioManager.Instance.playedDay + nodeOccurenceDate; //노드에 삽입되는 날 기준으로부터 노드 최소 발생 일자가 더해진 날짜를 저장, 클래스 생성 후 바로 리스트업할 경우 대비해 일단 값은 초기화 함
        this.nextScenarioNode = nextScenarioNode; //만약 다음 시나리오 노드가 인자로 제공되면 해당 노드로 다음 시나리오 노드 교체
    }


    public virtual void RunScenarioNode() //노드 진행용으로 쓰면 됨니다, 업데이트문에서 계속 실행시켜줌, 활성화된 노드가 너무 많으면 성능이 어떻게 될진 나도 모르겠다, 아마 여러 노드에서 같은 변수 참조해서 상태 판별할 때 실행 순서 꼬여서 잣될거 같긴 한데 어떻게든 되겠지
    {

    }

    public virtual void FlagChecker() // 특정 트리거 상태(플레이어가 특정 매체를 봤거나 겪었는지) 판별하는 메서드, 근데 구현해보니 RunScenarioNode()에 합쳐도 될 것 같긴 한데 특정 상황에서만 판별할 일이 있을 수 도 있으니 일단 빼놈
    {

    }

    public ScenarioNode NodeEjector() //다음 노드 return으로 발사하기
    {
        if(nextScenarioNode == null)
        {
            return null; //없으면 null값 발사, gpt가 참조형은 리스트에서 add()해도 그냥 넘어간다고 했으니 ㄱㅊ
        }
        nextScenarioNode.MinNodeStartDateSetter(); //다음 노드 minNodeStartDate 값 오늘로 갱신

        return nextScenarioNode; //발사!
    }

    public void MinNodeStartDateSetter() //리스트에 삽입 직전 minNodeStartDate 날짜 데이터 갱신, 이전 노드에서 실행시킨다
    {
        minNodeStartDate = ScenarioManager.Instance.playedDay + nodeOccurenceDate;
    }

}


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
}

public class ScenarioNode_2 : ScenarioNode //2번 시나리오 노드
{
    public ScenarioNode_2(int nodeID, int nodeOccurenceDate, float occurenceProbability) : base(nodeID, nodeOccurenceDate, occurenceProbability)
    {
        this.nextScenarioNode = new ScenarioNode_3(2, 2, 0.1f); //3번 시나리오 노드 id 2, 2일 공백, 10%확률로 실행
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
}

public class ScenarioNode_3 : ScenarioNode //3번 시나리오 노드
{
    public ScenarioNode_3(int nodeID, int nodeOccurenceDate, float occurenceProbability) : base(nodeID, nodeOccurenceDate, occurenceProbability)
    {
        this.nextScenarioNode = null;
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
}