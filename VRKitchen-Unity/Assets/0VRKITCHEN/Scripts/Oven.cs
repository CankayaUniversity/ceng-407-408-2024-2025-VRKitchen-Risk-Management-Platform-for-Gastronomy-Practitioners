using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OvenController : MonoBehaviour
{
    //public GameObject heatingArea; // Assign the heating area GameObject
    public bool isOvenOn = false;
    

   
    public void ToggleOven()
    {
        isOvenOn = !isOvenOn;
        

        if (isOvenOn)
        {
            Debug.Log("Oven is ON. Heating objects...");//PROMPT ATILCAK YER
            
        }
        else
        {
            Debug.Log("Oven is OFF.");//PROMPT ATILCAK YER

        }
    }

    
}