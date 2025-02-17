using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
    public class FrequencySound //라디오 오디오 덩어리
    {
        public float targetFrequency; // 목표 주파수
        public float frequencyRange;  // 주파수 범위 (±값)
        public AudioSource audioSource; // 재생할 사운드
        public bool playTrigger; //재생 가능한 오디오인지 확인용
        public bool isPlayedEnough = false; //충분히 플레이어가 재생하여 정보를 얻을 정도의 시간이 흘렀는지
        public float playedTime = 0f;
        public float playedEnoughTime;
        public void PlayTimeChecker()
        {
            playedTime += Time.deltaTime;
        }

        public void Init() //오디오를 만약 두번 이상 재사용을 해서 불러오려면 노드 종료할 때 이거 한번씩 돌려주세요
        {
            playTrigger  = false;
            isPlayedEnough = false;
            playedTime = 0f;
        }
    }

public class RadioManagerScript : MonoBehaviour
{
    private static RadioManagerScript instance = null;

    public bool isactivated = false;

    [Header("라디오 최대 최소 주파수")]
    public float minFrequency = 88.0f;
    public float maxFrequency = 108.0f;

    [Header("현재 라디오 주파수")]
    public float radioFrequency = 0f;

    public bool isDragging = false; //라디오 휠이 드래그 중인지지

    public float ratioChangeSpeed = 1f; //주파수 변화 속도

    public string csvFilePath = "Radio Audio Resources/Radio Sound Table"; //라디오 오디이 CSV 파일 경로로

    public List<FrequencySound> frequencySounds; // 라디오 오디오 전체 리스트
    public AudioSource radioNoise; //기본 라디오 노이즈

    public List<FrequencySound> playFrequencySoundsList; //실제 재생하는 라디오 오디오오 리스트, 라디오 방송 추가, 삭제를 위해 리스트로 관리

    public int tempDayCount=0; //라디오 방송 관리 위한 임시 게임 진행 일자 변수

    public bool updateRadio = false; //라디오 업데이트문 실행 여부
    
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

    public static RadioManagerScript Instance
    {
        get
        {
            if(instance == null)
                return null;
            return instance;
        }
    }

    void Start()
    {
        isactivated = false;
    }

    void Update()
    {
        
    }

    
}
