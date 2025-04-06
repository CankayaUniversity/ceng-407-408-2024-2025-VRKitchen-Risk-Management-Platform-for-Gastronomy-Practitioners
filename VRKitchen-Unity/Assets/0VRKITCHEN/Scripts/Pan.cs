using System.Collections.Generic;
using UnityEngine;

public class Pan : MonoBehaviour
{
    public float temperature = 10f;
    public List<GameObject> foodItems = new List<GameObject>(); // List of food items in the pan
    public float heatingTemperature = 10f; // Temperature increase per second

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Food")) // Check if the object is food
        {
            Debug.Log("Food collided with pan: " + collision.gameObject.name);
            AddFood(collision.gameObject);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Food")) // Check if the object is food
        {
            Debug.Log("Food exited pan collision: " + collision.gameObject.name);
            RemoveFood(collision.gameObject);
        }
    }

    private void AddFood(GameObject food)
    {
        if (!foodItems.Contains(food))
        {
            foodItems.Add(food); // Add food to the list
        }
    }

    private void RemoveFood(GameObject food)
    {
        if (foodItems.Contains(food))
        {
            foodItems.Remove(food); // Remove food from the list
        }
    }

    public void Heat(float amount)
    {
        temperature += amount * Time.deltaTime;
        Debug.Log("Pan temperature: " + temperature);

        // Optionally, apply heat effects to food items
        foreach (var food in foodItems)
        {
            Food foodScript = food.GetComponent<Food>();
            if (foodScript != null)
            {
                foodScript.Heat(heatingTemperature * Time.deltaTime);
            }
        }
    }
}
