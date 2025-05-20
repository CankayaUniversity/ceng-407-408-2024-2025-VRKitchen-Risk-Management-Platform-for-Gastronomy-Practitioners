using UnityEngine;

public class NonCookableFoodInstance : MonoBehaviour
{
    public FoodData foodData;
    private void Awake()
    {
        if (foodData == null || foodData.isCookable)
        {
            Debug.LogWarning("NonCookableFoodInstance attached to a cookable item.");
            enabled = false;
        }
    }
}