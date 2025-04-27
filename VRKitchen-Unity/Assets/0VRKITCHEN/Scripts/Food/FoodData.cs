using UnityEngine;

[CreateAssetMenu(fileName = "FoodData", menuName = "ScriptableObjects/Food")]
public class FoodData : ScriptableObject
{
    public string foodName;
    public Sprite foodImage;
    public GameObject foodPrefab;

    [Header("Food Properties")]
    public int calories;
    public bool isHazardous;
    public string foodCategory;
    public string foodDescription;

    [Header("Cooking Properties")]
    public bool isCookable;
    public float idealCookingTime;      // e.g. 10 seconds
    public float burnThresholdTime;     // e.g. 15 seconds
    public float requiredTemperature;
    // we can add other things later if needed
}
