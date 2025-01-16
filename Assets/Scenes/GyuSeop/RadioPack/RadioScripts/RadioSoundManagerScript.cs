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
        UpdateSoundVolumes();
    }

    private void UpdateSoundVolumes()
    {
        if(RadioManagerScript.Instance.isactivated) //라디오 켜지면 주파수에 따라 음량 조절 시작작
        {
            foreach (var frequencySound in RadioManagerScript.Instance.frequencySounds)
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
}