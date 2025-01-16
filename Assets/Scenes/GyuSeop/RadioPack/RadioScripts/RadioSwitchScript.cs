using UnityEngine;

public class RadioSwitchScript : MonoBehaviour
{
    void Start()
    {

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
            foreach (var tempFrequencySound in RadioManagerScript.Instance.frequencySounds) //라디오가 꺼져도 사운드는 계속 흐르는 경우
            {
                tempFrequencySound.audioSource.volume = 0;
            }
        }
        else //전원 off -> on
        {
            RadioManagerScript.Instance.radioNoise.Play();
            RadioManagerScript.Instance.isactivated=true;
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
        //GetComponent<SpriteRenderer>().color = Color.white;
    }
}
