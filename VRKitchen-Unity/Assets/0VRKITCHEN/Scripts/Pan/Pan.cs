using System.Collections.Generic;
using UnityEngine;

public class Pan : MonoBehaviour
{
    public float heatingTemperature = 0.05f;

    private List<FoodInstance> foodItems = new List<FoodInstance>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            FoodInstance food = other.GetComponent<FoodInstance>();
            if (food != null && !foodItems.Contains(food))
            {
                foodItems.Add(food);
                

                // Send RAG query
                var manager = FindObjectOfType<GameActionManager>();
                if (manager != null && food.foodData != null)
                {
                    string name = food.foodData.foodName.ToLower();
                    //manager.RegisterAction($"I placed the {name} in the pan. What now ?");
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            FoodInstance food = other.GetComponent<FoodInstance>();
            if (food != null && foodItems.Contains(food))
            {
                foodItems.Remove(food);
            }
        }
    }

    public void Heat(float amount)
    {
        foreach (var food in foodItems)
        {
            food.Heat(amount); // Applies heat directly to each FoodInstance
        }
    }
}