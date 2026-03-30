using UnityEngine;
using UnityEngine.UI;

public class MessegerUI : MonoBehaviour
{
    private string selectAnswerText;

    public void CheckButtonEvent()
    {
        if(TextsController.instance.GetSelectAnswerText() != null)
        {
            selectAnswerText = TextsController.instance.GetSelectAnswerText();

            if(selectAnswerText == "Hey..what's up?")
            {
                Debug.Log("Hey..what's up? 선택");
            }

            if(selectAnswerText == "Yeah, I'm here")
            {
                Debug.Log("Yeah, I'm here 선택");
            }
        }
    }
}
