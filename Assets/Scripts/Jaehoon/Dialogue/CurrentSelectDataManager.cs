//***************************
// 파일명: CurrentSelectDataManager.cs
// 작성자: 김재훈
// 작성일: 2026.03.23
// 내용: 씬 전환간 특정 데이터를 넘겨줘야 할 때 사용하기 위한 스크립트. ex) 스테이지 선택씬에서 1번 스테이지 선택 -> 전투 씬에서 1번 스테이지 웨이브 데이터 불러오기
//***************************

using UnityEngine;

public class CurrentSelectDataManager : Singleton<CurrentSelectDataManager>
{
    public int currentSelectMapNum; // 스테이지 선택씬 <-> 전투씬 간 데이터 교환에 필요한 변수
    public int currentSelectStoryNum; // 스토리 선택씬 <-> 대화씬 간 데이터 교환에 필요한 변수
    public bool storyContinuousProgress = false;
    public bool storyAutoProgress = false;

    public void SettingOnButtonCurrentSelectStage(int mapNum) // 스테이지 선택 씬에서 맵 입장 버튼에 연결 할 메서드
    {
        currentSelectMapNum = mapNum;
    }

    public void SettingOnButtonNextStage()
    {
        if(currentSelectMapNum!= 0)
        {
            currentSelectMapNum++;
        }
    }

    public void SettingOnButtonCurrentSelectStory(int storyNum) // 스테이지 선택 씬에서 맵 입장 버튼에 연결 할 메서드
    {
        currentSelectStoryNum = storyNum;
    }

    public void SettingOnButtonNextStory()
    {
        if(currentSelectStoryNum!= 0)
        {
            currentSelectStoryNum++;
        }
    }

    // public void SetStoryAutoProgress(bool value)
    // {
    //     storyAutoProgress = value;
    // }

    // public void SetStoryContinuousProgress(bool value)
    // {
    //     storyContinuousProgress = value;
    // }
}
