using UnityEngine;
using System.Collections.Generic;

public class Pan : MonoBehaviour
{
    public float heatingTemperature = 0.05f;
    public List<FoodInstance> foodItems = new List<FoodInstance>();

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Food"))
        {
            FoodInstance food = collision.gameObject.GetComponent<FoodInstance>();
            if (food != null && !foodItems.Contains(food))
            {
                foodItems.Add(food);
                
            }
        }
    }


    public void HeatPan(float amount)
    {
        foreach (var food in foodItems)
        {
            food.HeatFood(amount);
        }
    }
}