using System.Collections.Generic;
using UnityEngine;

public class Pan : MonoBehaviour
{
    public float temperature = 10f;
    public List<GameObject> foodItems = new List<GameObject>(); 
    public float heatingTemperature = 10f; 

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Food")) 
        {
            Debug.Log("Food collided with pan: " + collision.gameObject.name);
            AddFood(collision.gameObject);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Food")) 
        {
            Debug.Log("Food exited pan collision: " + collision.gameObject.name);
            RemoveFood(collision.gameObject);
        }
    }

    private void AddFood(GameObject food)
    {
        if (!foodItems.Contains(food))
        {
            Rigidbody foodRb = food.gameObject.GetComponent<Rigidbody>();
            if (foodRb != null && GetComponent<Rigidbody>() != null)
            {
                FixedJoint joint = food.gameObject.AddComponent<FixedJoint>();
                joint.connectedBody = GetComponent<Rigidbody>(); // tencere ile baðla
                joint.breakForce = Mathf.Infinity; // Gerekirse kýrýlabilir de ayarlanabilir
                AddFood(food.gameObject);
                foodItems.Add(food);
            }
            
        }
    }

    private void RemoveFood(GameObject food)
    {
        if (foodItems.Contains(food))
        {
            FixedJoint joint = food.gameObject.GetComponent<FixedJoint>();
            if (joint != null)
            {
                Destroy(joint);
            }
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
