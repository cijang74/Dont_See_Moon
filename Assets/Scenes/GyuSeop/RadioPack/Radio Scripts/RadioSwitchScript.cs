using UnityEngine;
using TMPro;

public class RadioSwitchScript : MonoBehaviour
{
    public TMP_Text frequencyDisplayColor; // 주파수 텍스트
    void Start()
    {
        frequencyDisplayColor.color=new Color32(60,60,60,255);
    }

    void Update()
    {
        
    }

    public void OnButtonPressed()
    {
        Debug.Log("Button pressed!");

        if(RadioManagerScript.Instance.isactivated) //전원 on -> off
        {
            RadioManagerScript.Instance.radioNoise.Pause();
            RadioManagerScript.Instance.isactivated=false;
            foreach (var tempFrequencySound in RadioManagerScript.Instance.playFrequencySoundsList) //라디오가 꺼져도 사운드는 계속 흐르는 경우, 끄거나 재생되지 않을 때 라디오를 정지시킬지는 추후 결정 필요요
            {
                tempFrequencySound.audioSource.volume = 0;
            }

            frequencyDisplayColor.color=new Color32(50,50,50,255);
        }
        else //전원 off -> on
        {
            RadioManagerScript.Instance.radioNoise.Play();
            RadioManagerScript.Instance.isactivated=true;

            frequencyDisplayColor.color=new Color32(255,255,255,255);
            ScenarioManager.Instance.DayChanger(); //시나리오 노드 매니저 테스트용 코드, 작동하면 날짜 하나 올라감
        }
    }

    private void OnMouseDown()
    {
        GetComponent<SpriteRenderer>().color = Color.gray;
        // 마우스 클릭 이벤트 처리
        OnButtonPressed();
    }

    private void OnMouseUp() 
    {
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    private void OnMouseEnter()
    {
        // 마우스가 버튼 위에 있을 때 색상 변경
        //GetComponent<SpriteRenderer>().color = Color.gray;
    }

    private void OnMouseExit()
    {
        // 마우스가 버튼에서 떠날 때 색상 복구
        //GetComponent<SpriteRenderer>().color = Color.white; frequencyDisplay
    }
}
