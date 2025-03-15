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
            toAPI.queryText = "Stove is on, what's the next step in the game? Give me one step.";
            toAPI.SubmitQuery();
        }
        else
        {
            Debug.Log("Oven is OFF.");//PROMPT ATILCAK YER
            toAPI.queryText = "Stove is off, what's the next step in the game? ";
            //toAPI.SubmitQuery();

        }
    }

    
}