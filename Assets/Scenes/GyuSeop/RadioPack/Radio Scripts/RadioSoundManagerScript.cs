using UnityEngine;
using System.Collections.Generic;

public class RadioSoundManagerScript : MonoBehaviour
{
    void Start()
    {
        LoadFrequencyData();
        SelectTodayFrequency(); //날짜 바뀌거나 재생 라디오 오디오 변경 시 호출 필요
    }

    void Update()
    {
        if(RadioManagerScript.Instance.updateRadio)
            UpdateSoundVolumes();
    }

    private void UpdateSoundVolumes() //이건 게임에서 하루가 계속 진행 중일 때 update에서 계속 돌려야 할 메서드
    {
        if(RadioManagerScript.Instance.isactivated) //라디오 켜지면 주파수에 따라 음량 조절 시작
        {
            for(int i = 0; i < RadioManagerScript.Instance.playFrequencySoundsList.Count; i++)
            {
                FrequencySound frequencySound = RadioManagerScript.Instance.playFrequencySoundsList[i];

                float distance = Mathf.Abs(RadioManagerScript.Instance.radioFrequency - frequencySound.targetFrequency);

                // 거리에 따라 볼륨 계산 (거리 0일 때 1, 범위 초과 시 0)
                if (distance <= frequencySound.frequencyRange)
                {
                    if(!frequencySound.audioSource.isPlaying)
                    {
                        frequencySound.audioSource.Play();
                    }

                    frequencySound.audioSource.volume = 1 - (distance / frequencySound.frequencyRange); //오디오 사운드 볼륨 높이기
                    RadioManagerScript.Instance.radioNoise.volume = 1 - frequencySound.audioSource.volume; //노이지 볼륨 줄이기
                }
                else
                {
                    frequencySound.audioSource.volume = 0; // 범위를 벗어나면 볼륨 0
                    frequencySound.audioSource.Pause();
                }

                if(frequencySound.audioSource.volume != 0) //볼륨이 켜져 있으면 플레이 시간 적립, ***만약 일정 볼륨 이상으로 커져야 카운트하려면 범위값으로 조건문 만들기
                    frequencySound.PlayTimeChecker();

                if(frequencySound.isPlayedEnough == false && frequencySound.playedEnoughTime < frequencySound.playedTime) //플레이충분트리거 false이고 플레이충분시간보다 플레이시간이 더 많으면 바꿔주기
                {
                    frequencySound.isPlayedEnough = true;
                }
            }
        }
    }

/*
노드 클래스에서 update문으로 계속 특정 라디오의 ture값 대입하고 있음, 특정 라디오의 euough변수는 날이 바뀌고 노드 플래그 체킹 메서드에서 체크 후 다음으로 진행, 오디오 리스트 갱신은 모두 끝나고 하루 시작 전에 시작하기
daychanger 돌아가면서 할 일 : 오디오 플레이되면 시간 체크해서 일정시간 이상 플레이되면 플래그 켜기 -> 플레이충분 변수값에 따라서 노드 엔드플래그 갱신 -> 다음날 노드들 모두 리스트에 갱신하기 -> 노드에서 특정 오디오 켜기 -> 오디오 리스트 true값에 따라 갱신 -> 다시 라디오 UPDATE문 진행
*/

    public void SelectTodayFrequency() //조건에 따라서 재생될 라디오 오디오 종류 결정. 이건 하루가 지난 후 날짜가 바뀐 후후 돌려야 할 메서드
    {
        foreach (var frequencySound in RadioManagerScript.Instance.playFrequencySoundsList)
        {
            frequencySound.audioSource.Pause();
            frequencySound.audioSource.volume = 0f;
        }

        RadioManagerScript.Instance.playFrequencySoundsList.Clear(); //재생되는 오디오 리스트 빈 리스트로 초기화
        
        RadioManagerScript.Instance.playFrequencySoundsList = RadioManagerScript.Instance.frequencySounds.FindAll(sound => sound.playTrigger); //재생 상태 TRUE인 모든 라디오 오디오를 재생 리스트에 추가
    /*
        foreach (var frequencySound in RadioManagerScript.Instance.playFrequencySoundsList)
        {
            frequencySound.audioSource.Play();
            frequencySound.audioSource.volume = 0f;
        }
    */
    }
    //현재 버그 발생하는 경우
    //현재 재생 중인 오디오가 있는 상황에서 날짜가 넘어가면 리스트를 비워버리면서 더이상 이전 리스트에 있던 오디오의 불륨 커트롤이 안되는 문제

    void LoadFrequencyData() //이건 게임 실행 시 최초 1회 실행되는 메서드
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
                playTrigger = bool.Parse(columns[3]),
                playedEnoughTime = float.Parse(columns[4])
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