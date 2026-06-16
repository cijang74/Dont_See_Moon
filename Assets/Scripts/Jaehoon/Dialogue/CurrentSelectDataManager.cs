// //***************************
// // 파일명: CurrentSelectDataManager.cs
// // 작성자: 김재훈
// // 작성일: 2026.03.23
// // 내용: 씬 전환간 특정 데이터를 넘겨줘야 할 때 사용하기 위한 스크립트. ex) 스테이지 선택씬에서 1번 스테이지 선택 -> 전투 씬에서 1번 스테이지 웨이브 데이터 불러오기
// //***************************

// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class CurrentSelectDataManager : Singleton<CurrentSelectDataManager>
// {
//     // 김예찬 추가
//     [Header ("예외 Scene 설정")]
//     [Tooltip ("뒤로가기 버튼으로 돌아가면 안 되는 씬 적어주세요")]
//     public List<string> ignoreSceneList = new List<string>{
//         "Scene_Deck", "Scene_PVPEnter", "Scene_PVPBattle", "Scene_1Stage_Battle", "Scene_Dialogue" };

//     [Header("CurrentSelectDataManager")]
//     public int currentSelectMapNum; // 스테이지 선택씬 <-> 전투씬 간 데이터 교환에 필요한 변수
//     public int currentSelectStoryNum; // 스토리 선택씬 <-> 대화씬 간 데이터 교환에 필요한 변수
//     public bool storyContinuousProgress = false;
//     public bool storyAutoProgress = false;
//     public bool storyHideUI = false;
//     // public User currentEnemyUser;
    
//     // public ShopType currentShopType; //상점 입장 시에 보여줄 탭
//     public bool isUsingNavagateButton = false;
//     public Stack<string> sceneLoute = new Stack<string>();
//     string startScene = "Scene_Start";

//     private string previousSceneName = "";

//     private void OnEnable()
//     {
//         SceneManager.sceneLoaded += SaveSceneLoute;
//     }

//     private void OnDisable()
//     {
//         SceneManager.sceneLoaded -= SaveSceneLoute;
//     }
//     protected override void OnDestroy() //2026. 05. 18. 강문석. 로그아웃 용으로 추가
//     {
//         base.OnDestroy();
//         SceneManager.sceneLoaded -= SaveSceneLoute;
//     }

//     void SaveSceneLoute(Scene loadedScene, LoadSceneMode mode)
//     {
//         //이전 씬이 없거나 시작 씬인 경우 저장 안 함
//         if (string.IsNullOrEmpty(previousSceneName) || previousSceneName == startScene)
//         {
//             previousSceneName = loadedScene.name;
//             return;
//         }

//         //무시할 씬으로 이동하는 경우 저장 안 함
//         if (ignoreSceneList.Contains(loadedScene.name))
//         {
//             previousSceneName = loadedScene.name;
//             return;
//         }

//         //BackButton으로 씬을 이동한 경우 저장 안 함
//         if (isUsingNavagateButton)
//         {
//             isUsingNavagateButton = false;
//             previousSceneName = loadedScene.name;
//             return;
//         }

//         //무시할 씬에서 이동하는 경우 스택을 재조정
//         if(ignoreSceneList.Contains(previousSceneName))
//         {
//             previousSceneName = loadedScene.name;
//             return;
//         }

//         //동일한 씬으로 이동하는 경우 저장 안 함.
//         if(previousSceneName == loadedScene.name)
//         {
//             previousSceneName = loadedScene.name;
//             return;
//         }

//         //이미 이동 기록이 있다면 저장 안 함,
//         if (0 < sceneLoute.Count && sceneLoute.Contains(loadedScene.name))
//         {
//             previousSceneName = loadedScene.name;
//             return;
//         }

//         //나머지 경우에는 이동하기 전 씬을 저장
//         sceneLoute.Push(previousSceneName);
//         previousSceneName = loadedScene.name;

//         /*
//         // 김예찬 추가 로직
//         // 이전 씬이 무시 리스트 + 불러온 씬이 무시 리스트가 아님 이라면
//         if(0 < sceneLoute.Count && !ignoreSceneList.Contains(loadedScene.name))
//         {
//             if(!ignoreSceneList.Contains(previousSceneName)) return;
//             string temp;
//             do
//             {
//                 // 스택 제거
//                 temp = sceneLoute.Pop();
//             } while(temp != loadedScene.name);

//             previousSceneName = loadedScene.name;
//             return;
//         }
//         /// 왜 이렇게 짰나 해설문
//         /// 되도록 기존 코드 안 건드린다고 이렇게 짬. 더 좋은 방법으로 바꿀거면 편히 지워줘요
//         // == 여기서부터 기존 코드
//         */
//     }

//     public void SettingOnButtonCurrentSelectStage(int mapNum) // 스테이지 선택 씬에서 맵 입장 버튼에 연결 할 메서드
//     {
//         currentSelectMapNum = mapNum;
//         Debug.Log("currentSelectMapNum:" + currentSelectMapNum);
//     }

//     public void SettingOnButtonNextStage()
//     {
//         if(currentSelectMapNum!= 0)
//         {
//             currentSelectMapNum++;
//         }
//     }

//     public void SettingOnButtonCurrentSelectStory(int storyNum) // 스테이지 선택 씬에서 맵 입장 버튼에 연결 할 메서드
//     {
//         currentSelectStoryNum = storyNum;
//     }

//     public void SettingOnButtonNextStory()
//     {
//         if(currentSelectStoryNum!= 0)
//         {
//             currentSelectStoryNum++;
//         }
//     }

//     // public void SetCuttentEnemyUser(User user)
//     // {
//     //     currentEnemyUser = user;
//     // }

//     // public void SetStoryAutoProgress(bool value)
//     // {
//     //     storyAutoProgress = value;
//     // }

//     // public void SetStoryContinuousProgress(bool value)
//     // {
//     //     storyContinuousProgress = value;
//     // }
// }
