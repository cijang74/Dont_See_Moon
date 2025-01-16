using UnityEngine;

public class RadioSoundManagerScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SelectTodayFrequency(); //일단 계속 없데이트, 추후 다음날로 진행 시에만 작동 필요
        UpdateSoundVolumes();
    }

    private void UpdateSoundVolumes()
    {
        if(RadioManagerScript.Instance.isactivated) //라디오 켜지면 주파수에 따라 음량 조절 시작작
        {
            foreach (var frequencySound in RadioManagerScript.Instance.playFrequencySoundsList)
            {
                // 현재 주파수와 목표 주파수 간의 거리 계산
                float distance = Mathf.Abs(RadioManagerScript.Instance.radioFrequency - frequencySound.targetFrequency);

                // 거리에 따라 볼륨 계산 (거리 0일 때 1, 범위 초과 시 0)
                if (distance <= frequencySound.frequencyRange)
                {
                    frequencySound.audioSource.volume = 1 - (distance / frequencySound.frequencyRange); //오디오 사운드 볼륨 높이기
                    RadioManagerScript.Instance.radioNoise.volume = 1 - frequencySound.audioSource.volume; //노이지 볼륨 줄이기
                }
                else
                {
                    frequencySound.audioSource.volume = 0; // 범위를 벗어나면 볼륨 0
                }
            }
        }
        else
        {
            
        }
    }

    private void SelectTodayFrequency() //플레이 일차 재생될 라디오 주파수 결정
    {
        RadioManagerScript.Instance.playFrequencySoundsList.Clear(); //빈 리스트로 초기화

        switch(RadioManagerScript.Instance.tempDayCount)
        {
            case 0:
                RadioManagerScript.Instance.playFrequencySoundsList.Add(RadioManagerScript.Instance.frequencySounds[0]);
                break;
            case 1:
                RadioManagerScript.Instance.playFrequencySoundsList.Add(RadioManagerScript.Instance.frequencySounds[1]);
                break;
            case 2:
                RadioManagerScript.Instance.playFrequencySoundsList.Add(RadioManagerScript.Instance.frequencySounds[2]);
                break;
        }
    }
    //현재 버그 발생하는 경우
    //현재 재생 중인 오디오가 있는 상황에서 날짜가 넘어가면 리스트를 비워버리면서 더이상 커트롤이 안되는 문제
}