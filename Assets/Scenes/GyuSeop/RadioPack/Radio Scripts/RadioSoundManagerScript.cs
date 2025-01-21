using UnityEngine;
using System.Collections.Generic;

public class RadioSoundManagerScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadFrequencyData();
        SelectTodayFrequency(); //날짜 바뀌거나 재생 라디오 오디오 변경 시 호출 필요

        foreach (var frequencySound in RadioManagerScript.Instance.playFrequencySoundsList)
        {
            frequencySound.audioSource.Play();
            frequencySound.audioSource.volume = 0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //SelectTodayFrequency(); //일단 계속 없데이트, 추후 다음날로 진행 시에만 작동 필요
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

    private void SelectTodayFrequency() //조건에 따라서 재생될 라디오 오디오 종류 결정
    {
        RadioManagerScript.Instance.playFrequencySoundsList.Clear(); //빈 리스트로 초기화

        if(RadioManagerScript.Instance.tempDayCount == 1)
        {
            RadioManagerScript.Instance.frequencySounds[1].playTrigger = true;
        }
        else if(RadioManagerScript.Instance.tempDayCount == 2)
        {
            RadioManagerScript.Instance.frequencySounds[0].playTrigger = false;
            RadioManagerScript.Instance.frequencySounds[2].playTrigger = true;
        }
        
        RadioManagerScript.Instance.playFrequencySoundsList = RadioManagerScript.Instance.frequencySounds.FindAll(sound => sound.playTrigger); //재생 상태 TRUE인 모든 라디오 오디오를 재생 리스트에 추가
        
    }
    //현재 버그 발생하는 경우
    //현재 재생 중인 오디오가 있는 상황에서 날짜가 넘어가면 리스트를 비워버리면서 더이상 이전 리스트에 있던 오디오의 불륨 커트롤이 안되는 문제

    void LoadFrequencyData()
    {
        
        RadioManagerScript.Instance.frequencySounds = new List<FrequencySound>();
        TextAsset csvFile = Resources.Load<TextAsset>(RadioManagerScript.Instance.csvFilePath);

        if (csvFile == null)
        {
            Debug.LogError("CSV file not found at: " + RadioManagerScript.Instance.csvFilePath);
            return;
        }

        string[] rows = csvFile.text.Split('\n'); // 행 단위로 나누기
        for (int i = 1; i < rows.Length; i++) // 첫 줄은 헤더이므로 제외
        {
            if (string.IsNullOrWhiteSpace(rows[i])) continue; // 빈 줄은 스킵

            string[] columns = rows[i].Split(',');
            FrequencySound sound = new FrequencySound
            {
                targetFrequency = float.Parse(columns[0]),
                frequencyRange = float.Parse(columns[1]),
                playTrigger = bool.Parse(columns[3])
            };

            // AudioSource 생성 및 AudioClip 할당
            GameObject soundObject = new GameObject($"AudioSource_{i}");
            sound.audioSource = soundObject.AddComponent<AudioSource>();
            sound.audioSource.clip = Resources.Load<AudioClip>(columns[2]);
            sound.audioSource.playOnAwake = false; //초기 설정 초기화
            sound.audioSource.loop = true;

            if (sound.audioSource.clip == null)
            {
                Debug.LogError($"AudioClip not found at path: {columns[2]}");
                continue;
            }

            RadioManagerScript.Instance.frequencySounds.Add(sound);
        }
    }
}