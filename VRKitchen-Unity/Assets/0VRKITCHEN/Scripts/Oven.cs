using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OvenController : MonoBehaviour
{
    //public GameObject heatingArea; // Assign the heating area GameObject
    public bool isOvenOn = false;
    public UnityToAPI toAPI;

   
    public void ToggleOven()
    {
        isOvenOn = !isOvenOn;
        

        if (isOvenOn)
        {
            Debug.Log("Oven is ON. Heating objects...");//PROMPT ATILCAK YER
            toAPI.queryText = "oven is open";
            toAPI.SubmitQuery();
        }
        else
        {
            Debug.Log("Oven is OFF.");//PROMPT ATILCAK YER
            toAPI.queryText = "oven is closed";
            toAPI.SubmitQuery();

        }
    }

    
}