using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OvenController : MonoBehaviour
{
    
    public bool isOvenOn = false;
    public UnityToAPI toAPI;
    //public GameObject fireEffect;
    public float heatingTemperature = 100f;
    private List<GameObject> heatedObjects = new List<GameObject>();
    private void Start()
    {
        
        //if (fireEffect != null)
        //{
        //    fireEffect.SetActive(false); // Ensure fire effect is off initially
        //}
    }
   
    public void ToggleOven()
    {
        isOvenOn = !isOvenOn;
        

        if (isOvenOn)
        {
            Debug.Log("Oven is ON. Heating objects...");
            toAPI.queryText = "Stove is on, what's the next step in the game? Give me one step.";
            toAPI.SubmitQuery();
            
            //if (fireEffect != null)
            //{
            //    fireEffect.SetActive(true); // Turn on fire effect
            //}
        }
        else
        {
            Debug.Log("Oven is OFF.");
            toAPI.queryText = "Stove is off, what's the next step in the game? ";
            //toAPI.SubmitQuery();
            
            //if (fireEffect != null)
            //{
            //    fireEffect.SetActive(false); // Turn off fire effect
            //}

        }

    }

    public void HeatObject(GameObject obj)
    {
        if (obj != null && !heatedObjects.Contains(obj))
        {
            Debug.Log("Heating object: " + obj.name);
            heatedObjects.Add(obj);

            Pan pan = obj.GetComponent<Pan>();
            if (pan != null)
            {
                pan.Heat(heatingTemperature);
            }
        }
    }

    public void StopHeatingObject(GameObject obj)
    {
        if (obj != null && heatedObjects.Contains(obj))
        {
            Debug.Log("Stopped heating object: " + obj.name);
            heatedObjects.Remove(obj);

            Pan pan = obj.GetComponent<Pan>();
            if (pan != null)
            {
                pan.Cool(heatingTemperature);
            }
        }
    }


}