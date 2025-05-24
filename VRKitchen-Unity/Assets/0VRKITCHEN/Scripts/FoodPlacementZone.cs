using UnityEngine;

public class FoodPlacementZone : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        FoodInstance cookableFood = collision.gameObject.GetComponent<FoodInstance>();
        NonCookableFoodInstance nonCookableFood = collision.gameObject.GetComponent<NonCookableFoodInstance>();

        string zoneName = gameObject.name.ToLower().Replace("(clone)", "").Trim();
        string action = "";

        if (cookableFood != null && cookableFood.foodData != null)
        {
            action = $"I placed the {cookableFood.currentState.ToString().ToLower()} {cookableFood.foodData.foodName.ToLower()} on the {zoneName}. What now?";
        }
        else if (nonCookableFood != null && nonCookableFood.foodData != null)
        {
            action = $"I placed the {nonCookableFood.foodData.foodName.ToLower()} on the {zoneName}. What now?";
        }
        
        if (!string.IsNullOrEmpty(action))
        {
            Debug.Log(action);
            GameActionManager manager = FindObjectOfType<GameActionManager>();
            if (manager != null)
            {
                manager.RegisterAction(action);
            }
        }
    }

}