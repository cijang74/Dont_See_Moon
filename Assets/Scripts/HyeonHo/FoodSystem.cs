using UnityEngine;

public class FoodSystem : MonoBehaviour
{

    [SerializeField] private GameObject food;

    [SerializeField] Transform[] storeablePoints = new Transform[6];

    private GameObject[] storedObjects = new GameObject[6];

    [SerializeField] private Transform refrigeator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StoreFood()
    {
        for(int i = 0 ; i < storedObjects.Length; i++)
        {
            if(storedObjects[i] == null)
            {
                Vector3 editPos = storeablePoints[i].position;
                editPos.y += food.transform.localScale.z * 2;   //? 사물기준 위아래 : z , 좌표기준 위아래 : y 인듯

                GameObject newFood = Instantiate(food, editPos, storeablePoints[i].rotation, refrigeator);

                storedObjects[i] = newFood;

                break;
            }
        }
    }
}
