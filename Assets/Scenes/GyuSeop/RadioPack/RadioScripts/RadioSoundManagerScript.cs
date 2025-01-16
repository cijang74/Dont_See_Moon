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

    private void SelectTodayFrequency() //플레이 일차에 따라서서 재생될 라디오 주파수 종류 결정
    {
        RadioManagerScript.Instance.playFrequencySoundsList.Clear(); //빈 리스트로 초기화

        switch(RadioManagerScript.Instance.tempDayCount) //일차수에 따라서 리스트 내용물 갈아버리기기
        {
            case 0:
                RadioManagerScript.Instance.playFrequencySoundsList.Add(RadioManagerScript.Instance.frequencySounds[0]);
                break;
            case 1:
                RadioManagerScript.Instance.playFrequencySoundsList.Add(RadioManagerScript.Instance.frequencySounds[0]);
                RadioManagerScript.Instance.playFrequencySoundsList.Add(RadioManagerScript.Instance.frequencySounds[1]);
                break;
            case 2:
                RadioManagerScript.Instance.playFrequencySoundsList.Add(RadioManagerScript.Instance.frequencySounds[0]);
                RadioManagerScript.Instance.playFrequencySoundsList.Add(RadioManagerScript.Instance.frequencySounds[1]);
                RadioManagerScript.Instance.playFrequencySoundsList.Add(RadioManagerScript.Instance.frequencySounds[2]);
                break;
        }
    }
    //현재 버그 발생하는 경우
    //현재 재생 중인 오디오가 있는 상황에서 날짜가 넘어가면 리스트를 비워버리면서 더이상 이전 리스트에 있던 오디오의 불륨 커트롤이 안되는 문제

    private void InsertSeriesFrequency() //특정 주파수 재생 후 일정 일수 이후 연계 주파수가 재생되도록 삽입
    {
        
    }
}