using System.Collections.Generic;
using UnityEngine;

public class Pan : MonoBehaviour
{
    public float temperature = 10f;
    public List<GameObject> foodItems = new List<GameObject>(); 
    public float heatingTemperature = 10f; 


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food")) 
        {
            Debug.Log("Food placed in pan: " + other.name);
            
            AddFood(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Food")) 
        {
            Debug.Log("Food removed from pan: " + other.name);
           
            RemoveFood(other.gameObject);
        }
    }

    private void AddFood(GameObject food)
    {
        if (!foodItems.Contains(food))
        {
            foodItems.Add(food); 
        }
    }

    private void RemoveFood(GameObject food)
    {
        if (foodItems.Contains(food))
        {
            foodItems.Remove(food); 
        }
    }



    public void Heat(float amount)
    {
        temperature += amount * Time.deltaTime;
        Debug.Log("Pan temperature: " + temperature);

        
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