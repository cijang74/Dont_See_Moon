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
    }

    public FrequencySound[] frequencySounds; // 목표 주파수와 사운드 배열
    public AudioSource radioNoise; //기본 라디오 노이즈

    FrequencySound[] frequencySoundsQueue; //실제 재생하는 라디오 사운드
    
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
