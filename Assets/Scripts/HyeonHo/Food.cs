using UnityEngine;

public class Food : MonoBehaviour
{

    [SerializeField] private float foodHunger = 0;

    private HungerSystem hungerSystem;
    

    void Awake()
    {
        hungerSystem = GameObject.Find("EventSystem").GetComponent<HungerSystem>();
    }
    
    
    void Start()
    {
        
    }


    void Update()
    {
        
    }

    void OnMouseUp()
    {
        Debug.Log("밥을 먹었습니다.");
        EatFood();
    }

    private void EatFood()
    {
        Debug.Log("허기 증가량 : " + foodHunger);
        hungerSystem.IncreaseHungerAfterEat(foodHunger);
        Destroy(gameObject);
    }
}
