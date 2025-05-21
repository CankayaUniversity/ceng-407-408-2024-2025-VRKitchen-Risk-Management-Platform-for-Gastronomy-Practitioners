using UnityEngine;

public class Seasoning : MonoBehaviour
{
    private string seasoningType;

    private void Start()
    {
        seasoningType = gameObject.tag;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            FoodInstance food = other.GetComponent<FoodInstance>();
            if (food != null)
            {
                food.AddSeasoning(seasoningType, 1); 
            }

            gameObject.SetActive(false); // Return to pool or disable
        }
    }
}