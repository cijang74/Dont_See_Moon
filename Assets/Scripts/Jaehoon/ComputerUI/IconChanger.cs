using UnityEngine;
using UnityEngine.UI;

public class IconChanger : MonoBehaviour
{
    [SerializeField] private Sprite newMessageIcon;
    [SerializeField] private Sprite defaultMessageIcon;
    private Image childImage;
    bool isNewMasseger_Mother = true;
    
    private void Start() 
    {
        Transform childTransform = transform.Find("Image");

        if (childTransform != null)
        {
            childImage = childTransform.GetComponent<Image>();
        }

        ChangeIcon();
    }

    public void ChangeIcon()
    {
        if(gameObject.name == "Button: Mother")
        {
            if(isNewMasseger_Mother == true)
            {
                childImage.sprite = newMessageIcon;
                isNewMasseger_Mother = false;
            }

            else
            {
                childImage.sprite = defaultMessageIcon;
            }
        }
    }
}
