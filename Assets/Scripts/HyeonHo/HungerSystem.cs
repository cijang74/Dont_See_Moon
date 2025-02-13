using UnityEngine;
using UnityEngine.UI;

public class HungerSystem : MonoBehaviour
{    
    [SerializeField] private Slider hungerSlider;

    [SerializeField] private float maxHunger = 100;

    [SerializeField] private float currentHunger;
    

    [Space(10)]
    [Header("허기 감소 관련 설정")]
    //허기 감소 최소치(허기 최대량의 %)
    [SerializeField] private float minDecreasedHungerPercent = 15;

    //허기 감소 최소지가 가능한 시간(초)
    [SerializeField] private float minDecreasedHungerTime = 120;

    //허기 감소 최대치(허기 최대량의 %)
    [SerializeField] private float maxDecreasedHungerPercent = 25;

    //허기 감소 최대치가 가능한 시간(초)
    [SerializeField] private float maxDecreasedHungerTime = 300;

    private float decreaseAmount;

    void Start()
    {
        Init();
    }


    void Update()
    {
        
    }

    private void Init()
    {
        hungerSlider.maxValue = maxHunger;
        currentHunger = maxHunger;
        hungerSlider.value = currentHunger;
    }

    public void DecreaseHungerAfterSleep(float time)
    {
        //허기 감소 최소
        if(time < minDecreasedHungerTime)
        {
            decreaseAmount = maxHunger * (minDecreasedHungerPercent / 100);
        }
        //허기 감소 최대
        else if(maxDecreasedHungerTime < time)
        {
            decreaseAmount = maxHunger * (maxDecreasedHungerPercent / 100);
        }

        //그 사이 시간일 때
        else
        {
            //시간이 지난 정도에 따라 허기 감소
            decreaseAmount = maxHunger * ((minDecreasedHungerPercent + 
                                    (time - minDecreasedHungerTime) / (maxDecreasedHungerTime - minDecreasedHungerTime)
                                     * (maxDecreasedHungerPercent - minDecreasedHungerPercent)) / 100);
        }

        Debug.Log("허기 감소량 : " + decreaseAmount);

        currentHunger -= decreaseAmount;

        //0이하 방지
        currentHunger = Mathf.Max(currentHunger, 0f);

        hungerSlider.value = currentHunger;
    }
}
