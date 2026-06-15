using UnityEngine;

public class DayEventManager : MonoBehaviour
{
    [Header("매니저 연결")]
    public DayTransitionManager dayTransitionManager;
    
    private int lastProcessedDay = -1;

    void Start()
    {
        if (dayTransitionManager == null)
        {
            dayTransitionManager = FindAnyObjectByType<DayTransitionManager>();
        }
    }

    void Update()
    {
        if (dayTransitionManager == null) return;
        
        // 라디오 매니저와 사운드 리스트가 초기화되었는지 확인
        if (RadioManagerScript.Instance == null || 
            RadioManagerScript.Instance.frequencySounds == null || 
            RadioManagerScript.Instance.frequencySounds.Count == 0) 
        {
            return;
        }

        // 날짜가 변경되었을 때 이벤트 갱신
        if (dayTransitionManager.currentDay != lastProcessedDay)
        {
            lastProcessedDay = dayTransitionManager.currentDay;
            UpdateRadioSoundsForDay(lastProcessedDay);
        }
    }

    private void UpdateRadioSoundsForDay(int day)
    {
        // 1, 2, 3일차에 해당하는 라디오 사운드 인덱스 결정
        int soundIndexToPlay = -1;
        if (day == 1) soundIndexToPlay = 0;
        else if (day == 2) soundIndexToPlay = 1;
        else if (day == 3) soundIndexToPlay = 2;

        bool changed = false;
        
        // 모든 주파수 사운드의 재생 트리거를 업데이트
        for (int i = 0; i < RadioManagerScript.Instance.frequencySounds.Count; i++)
        {
            bool shouldPlay = (i == soundIndexToPlay);
            if (RadioManagerScript.Instance.frequencySounds[i].playTrigger != shouldPlay)
            {
                RadioManagerScript.Instance.frequencySounds[i].playTrigger = shouldPlay;
                changed = true;
            }
        }

        // 변경사항이 적용되었거나 게임 시작 직후(첫날)일 경우 리스트 갱신
        if (changed || day == 1)
        {
            RadioSoundManagerScript rsm = FindAnyObjectByType<RadioSoundManagerScript>();
            if (rsm != null)
            {
                rsm.SelectTodayFrequency();
                if (soundIndexToPlay != -1)
                {
                    Debug.Log($"[DayEventManager] {day}일차가 되어 라디오 사운드 인덱스 {soundIndexToPlay}번이 재생 대기 상태가 되었습니다.");
                }
                else
                {
                    Debug.Log($"[DayEventManager] {day}일차에 할당된 라디오 사운드가 없습니다. 기존 재생이 중지됩니다.");
                }
            }
        }
    }
}
