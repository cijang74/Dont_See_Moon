// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public class FoldingMenu : MonoBehaviour
// {
//     [SerializeField] GameObject hiddenContentPanel;
//     [SerializeField] Image foldButtonImage;

//     [SerializeField] ScriptDataLoader scriptDataLoader;
//     // [SerializeField] ButtonController buttonController;
 
//     [SerializeField] GameObject conformPanel;
//     [SerializeField] TMP_Text conformPanel_Titletext;
//     [SerializeField] TMP_Text conformPanel_text;
//     [SerializeField] Button conformPanel_YesButton;
//     [SerializeField] Button conformPanel_NoButton;

//     [SerializeField] GameObject blackPanel;

//     // 현재 메뉴가 열려있는지 확인하는 변수
//     private bool isExpanded = false; 

//     // 버튼을 누를 때마다 실행될 함수
//     public void ToggleMenu()
//     {
//         // 상태를 반대로 뒤집음 (true -> false, false -> true)
//         isExpanded = !isExpanded;

//         // 패널을 켜거나 끔
//         hiddenContentPanel.SetActive(isExpanded);

//         // 상태에 따라 화살표 이미지 회전
//         foldButtonImage.transform.localRotation = isExpanded 
//             ? Quaternion.Euler(0, 0, 270) 
//             : Quaternion.Euler(0, 0, 180);
//     }

//     public void SetConformPanelToExit()
//     {
//         // conformPanel_Titletext.text = $"에피소드 {scriptDataLoader.episodeNum}({scriptDataLoader.title})";
//         conformPanel_text.text = "에피소드 감상을 종료하시겠습니까?";

//         conformPanel_YesButton.onClick.RemoveAllListeners();
//         conformPanel_YesButton.onClick.AddListener(() =>
//         {
//             // buttonController.LoadScene_NoLoad("Scene_ChapterSelect");
//         });

//         conformPanel_NoButton.onClick.RemoveAllListeners();
//         conformPanel_NoButton.onClick.AddListener(() =>
//         {
//             blackPanel.SetActive(false);
//         });

//         conformPanel.SetActive(true);
//     }

//     public void SetConformPanelToSkip()
//     {
//         // conformPanel_Titletext.text = $"에피소드 {scriptDataLoader.episodeNum}({scriptDataLoader.title})";
//         conformPanel_text.text = "이 에피소드를 스킵하시겠습니까?";

//         conformPanel_YesButton.onClick.RemoveAllListeners();
//         conformPanel_YesButton.onClick.AddListener(() =>
//         {
//             // buttonController.SkipEpisode();
//         });

//         conformPanel_NoButton.onClick.RemoveAllListeners();
//         conformPanel_NoButton.onClick.AddListener(() =>
//         {
//             blackPanel.SetActive(false);
//         });

//         conformPanel.SetActive(true);
//     }

//     public void SetConformPanelToContinue()
//     {
//         // conformPanel_Titletext.text = $"에피소드 {scriptDataLoader.episodeNum}({scriptDataLoader.title})";
//         conformPanel_text.text = "에피소드가 종료되었습니다. 계속 이어서 보시겠습니까?";

//         conformPanel_YesButton.onClick.RemoveAllListeners();
//         conformPanel_YesButton.onClick.AddListener(() =>
//         {
//             // buttonController.SkipEpisode();
//         });

//         conformPanel_NoButton.onClick.RemoveAllListeners();
//         conformPanel_NoButton.onClick.AddListener(() =>
//         {
//             // buttonController.LoadScene_NoLoad("Scene_ChapterSelect");
//         });

//         conformPanel.SetActive(true);
//     }
// }
