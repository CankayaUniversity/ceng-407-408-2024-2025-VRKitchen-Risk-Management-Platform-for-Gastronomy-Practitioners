using UnityEngine;

public class FoodPlacementZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            FoodInstance food = other.GetComponent<FoodInstance>();
            if (food != null)
            {
                // Use the name of this GameObject (e.g. Pan, CuttingBoard, Plate)
                string zoneName = gameObject.name.ToLower().Replace("(clone)", "").Trim();

                GameActionManager manager = FindObjectOfType<GameActionManager>();
                if (manager != null && food.foodData != null)
                {
                    string action = $"I placed the {food.foodData.foodName.ToLower()} on the {zoneName} What is the next step?";
                    manager.RegisterAction(action);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            FoodInstance food = other.GetComponent<FoodInstance>();
            if (food != null)
            {
                string zoneName = gameObject.name.ToLower().Replace("(clone)", "").Trim();

                GameActionManager manager = FindObjectOfType<GameActionManager>();
                if (manager != null && food.foodData != null)
                {
                    string action = $"I removed the {food.foodData.foodName.ToLower()} from the {zoneName} What is the next step?";
                    manager.RegisterAction(action);
                }
            }
        }
    }
}