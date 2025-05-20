using UnityEngine;
using UnityEngine.UIElements;

public class FoodPlacementZone : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        FoodInstance food = collision.gameObject.GetComponent<FoodInstance>();
        if (food != null && food.foodData != null)
        {
            string zoneName = gameObject.name.ToLower().Replace("(clone)", "").Trim();
            string action = $"I placed the {food.currentState} {food.foodData.foodName.ToLower()} on the {zoneName}.What now?";

            Debug.Log(action);

            GameActionManager manager = FindObjectOfType<GameActionManager>();
            if (manager != null)
            {
                manager.RegisterAction(action);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        FoodInstance food = collision.gameObject.GetComponent<FoodInstance>();
        if (food != null && food.foodData != null)
        {
            string zoneName = gameObject.name.ToLower().Replace("(clone)", "").Trim();
            string action = $"I removed the {food.currentState} {food.foodData.foodName.ToLower()} from the {zoneName}. What now?";

            Debug.Log(action);

            GameActionManager manager = FindObjectOfType<GameActionManager>();
            if (manager != null)
            {
                manager.RegisterAction(action);
            }
        }
    }
}