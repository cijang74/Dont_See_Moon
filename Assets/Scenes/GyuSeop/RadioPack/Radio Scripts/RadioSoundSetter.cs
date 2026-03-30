using System.Collections.Generic;
using UnityEngine;

public class RadioSoundSetter : MonoBehaviour
{
    void Awake()
    {
        //LoadFrequencyData();
    }

    /*void LoadFrequencyData()
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
    }*/
}
