using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    private int day;

    private int GetDate()
    {
        return GameObject.Find("Bed").GetComponent<BedEvent>().GetDay();
    }
    
    void Start()
    {
        day = GetDate();
        Debug.Log(day);
    }
    
    void Update()
    {
        
    }
}
