using UnityEngine;
using UnityEngine.InputSystem.OSX;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ScenarioManager : MonoBehaviour
{
    private static ScenarioManager instance = null;

    public bool A = false;
    public bool B = false;
    public bool C = false;
    public bool D = false; //특정 매체나 방문자 등등 여러 이벤트들 실행되는지 판별 변수, 해당 변수가 참이 되어야 해당 이벤트 실행

    public List<GameObject> eventList; //여기에 어떤 이벤트가 일어났는지 판별하는 스크립트 붙은 오브젝트들 넣기

    void Awake() 
    {
        if(instance == null)
        {
            instance =this;
            DontDestroyOnLoad(this.gameObject); //현재 최상단 부모 오브젝트에 스크립트가 붙어있지 않기 때문에 작동 안됨, 추후 필요 시 부모 오브젝트도 dontdestroyonload 적용 필요
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public static ScenarioManager Instance
    {
        get
        {
            if(instance == null)
                return null;
            return instance;
        }
    }

    public int playedDay=0; //진행일차
    void Start()
    {
        
    }

    
    void Update()
    {

    }
}



public class Scenario
{
    public int scenarioID; //시나리오 ID
    public int scenarioOccurenceDate; //시나리오 최소 발생 일자
    public float occurenceProbability; //시나리오 발생 확률, 발생 일자에 확정으로 하려면 1로 하시오
    public bool scenarioEndFlag; //시나리오 종료 플래그
    public List<ScenarioNode> scenarioNode; //시나리오 노드 관리용

    Scenario(int scenarioID, int scenarioOccurenceDate, float occurenceProbability, bool scenarioEndFlag, List<ScenarioNode> scenarioNode)
    {
        this.scenarioID = scenarioID;
        this.scenarioOccurenceDate = scenarioOccurenceDate;
        this.occurenceProbability = occurenceProbability;
        this.scenarioEndFlag= scenarioEndFlag;
        this.scenarioNode = scenarioNode;
    }

    public bool IsScenarioOccurence() //확률적으로 해당 시나리오 실행, 안될 시 확률 가충치 부여
    {
        if(scenarioOccurenceDate <= ScenarioManager.Instance.playedDay)
        {
            if(Random.value < occurenceProbability)
            {
                return true;
            }
            else
            {
                occurenceProbability += 0.6f;
                return false;
            }
        }
        return false;
    }

    void RunScenarioNode()
    {

    }


}

public class ScenarioNode
{
    public int nodeID; //노드 ID
    public bool isFinish; //노드 실행 끝났는지
    

    ScenarioNode()
    {
        
    }

    void IsFinished()
    {
        
    }
}

