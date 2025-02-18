using UnityEngine;
using UnityEngine.UI;

public class NewsUI : MonoBehaviour
{
    [SerializeField] private GameObject newsCanvas;
    [SerializeField] private string[] newsHeadLines;
    [SerializeField] private Sprite[] newsSprites;
    [SerializeField] private TextAsset[] newsTexts;

    [SerializeField] private Text newsTextTitleText;
    [SerializeField] private OffAxisStudios.NewsFeed newsfeedComponent;
    [SerializeField] private Image newsImage1Image;

    public void ChangeNewsContents(int newsID)
    {
        if (newsID == 0)
        {
            newsTextTitleText.text = newsHeadLines[0];
            newsfeedComponent.myNewsText = newsTexts[0];
            newsfeedComponent.newsFeed.text = newsTexts[0].text;
            newsImage1Image.sprite = newsSprites[0];
        }
    }
}
