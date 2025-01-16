using System.Collections.Generic;
using UnityEngine;

public class RadioManagerScript : MonoBehaviour
{
    private static RadioManagerScript instance = null;

    public bool isactivated = false;

    [Header("라디오 최대 최소 주파수")]
    public float minFrequency = 88.0f;
    public float maxFrequency = 108.0f;

    [Header("현재 라디오 주파수")]
    public float radioFrequency = 0f;

    public bool isDragging = false;

    public float ratioChangeSpeed = 1f; //주파수 변화 속도

    [System.Serializable]
    public class FrequencySound //라디오 오디오 덩어리
    {
        public float targetFrequency; // 목표 주파수
        public float frequencyRange;  // 주파수 범위 (±값)
        public AudioSource audioSource; // 재생할 사운드
        public int playedCount; //해당 오디오가 송출된 횟수
    }

    public FrequencySound[] frequencySounds; // 목표 주파수와 사운드 배열
    public AudioSource radioNoise; //기본 라디오 노이즈

    public List<FrequencySound> playFrequencySoundsList; //실제 재생하는 라디오 사운드 리스트, 라디오 방송 추가, 삭제를 위해 리스트로 관리

    public int tempDayCount=0; //라디오 방송 관리 위한 임시 게임 진행 일자 변수
    
    void Awake() 
    {
        if(instance == null)
        {
            instance =this;
            DontDestroyOnLoad(this.gameObject);
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
        /*
        foreach(var tempFrequencySound in frequencySounds)
        {
            tempFrequencySound.audioSource.volume=0;
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
