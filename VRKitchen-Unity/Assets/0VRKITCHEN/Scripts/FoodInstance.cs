using System;
using UnityEngine;

// we are gonna add this script to the food prefabs
public enum CookingState
{
    Raw,
    Cooking,
    Cooked,
    Overcooked,
    Burnt
}

public class FoodInstance : MonoBehaviour
{
    public FoodData foodData;
    public CookingState currentState = CookingState.Raw;
    
    public Material rawMaterial;
    public Material cookedMaterial;
    public Material overcookedMaterial;
    public Material burntMaterial;

    private Renderer foodRenderer;

    private float cookingTimer = 0f;
    private bool isCooking = false;

    private void Awake()
    {
        foodRenderer = GetComponent<Renderer>();
        if(foodRenderer != null && rawMaterial != null)
        {
            foodRenderer.material = rawMaterial;
        }
    }

    public void StartCooking()
    {
        if (!foodData.isCookable) return;

        isCooking = true;
        currentState = CookingState.Cooking;
    }

    public void StopCooking()
    {
        isCooking = false;
    }

    private void Update()
    {
        if (!isCooking || !foodData.isCookable) return;

        cookingTimer += Time.deltaTime;
        UpdateCookingState();
    }

    private void UpdateCookingState()
    {
        if (cookingTimer < foodData.idealCookingTime)
        {
            currentState = CookingState.Cooking;
            if (foodRenderer.material != rawMaterial)
                foodRenderer.material = rawMaterial;
        }
        else if (cookingTimer < foodData.burnThresholdTime)
        {
            currentState = CookingState.Cooked;
            if (foodRenderer.material != cookedMaterial)
                foodRenderer.material = cookedMaterial;
        }
        else if (cookingTimer < foodData.burnThresholdTime + 5f)
        {
            currentState = CookingState.Overcooked;
            if (foodRenderer.material != overcookedMaterial)
                foodRenderer.material = overcookedMaterial;
        }
        else
        {
            currentState = CookingState.Burnt;
            if (foodRenderer.material != burntMaterial)
                foodRenderer.material = burntMaterial;
        }
    }


    public float GetCookingProgress()
    {
        return Mathf.Clamp01(cookingTimer / foodData.burnThresholdTime);
    }
}