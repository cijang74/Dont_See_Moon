using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace OffAxisStudios
{
    public class NewsFeed : MonoBehaviour
    {
        // Off Axis Studios 메뉴에 'News Patch Feed'를 추가
        [AddComponentMenu("Off Axis Studios/News Patch Feed")]

        string myText; // 뉴스 데이터를 저장할 변수

        public UnityEngine.UI.Text newsFeed; // 뉴스 피드 텍스트를 표시할 UI 요소
        public string myNewsURL; // 뉴스를 가져올 URL
        public string noNewsText; // 뉴스가 없을 때 표시할 기본 텍스트

        void Start()
        {
            StartCoroutine(GetNews()); // 뉴스 데이터를 가져오는 코루틴 실행
        }

        private IEnumerator GetNews()
        {
            // UnityWebRequest를 사용하여 URL에서 데이터 요청
            var feed = new UnityWebRequest(myNewsURL);
            feed.downloadHandler = new DownloadHandlerBuffer(); // 데이터 버퍼 초기화
            yield return feed.SendWebRequest(); // 요청을 보내고 응답을 대기

            myText = feed.downloadHandler.text; // 응답 데이터를 텍스트로 저장
            newsFeed.text = myText; // UI 텍스트에 응답 데이터 표시

            if (string.IsNullOrEmpty(newsFeed.text)) // 응답 데이터가 비었는지 확인
            {
                newsFeed.text = noNewsText; // 비어 있다면 기본 텍스트 표시
            }
        }
    }
}