using UnityEngine;

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

    public float temperature = 0f;

    public Material rawMaterial;
    public Material cookedMaterial;
    public Material overcookedMaterial;
    public Material burntMaterial;

    private Renderer foodRenderer;
    private Color originalColor;

    private void Awake()
    {
        foodRenderer = GetComponent<Renderer>();
        if (foodRenderer != null)
        {
            originalColor = foodRenderer.material.color;
            if (rawMaterial != null)
                foodRenderer.material = rawMaterial;
        }
    }

    private void Update()
    {
        if (!foodData.isCookable) return;

        UpdateCookingState();
    }

    public void Heat(float amount)
    {
        if (!foodData.isCookable) return;

        temperature += amount * Time.deltaTime;
    }

    private void UpdateCookingState()
    {
        // You can either use materials (realistic) or just color change (simplified)
        if (temperature < foodData.requiredTemperature)
        {
            currentState = CookingState.Cooking;
            SetMaterial(rawMaterial);
        }
        else if (temperature < foodData.burnThresholdTime)
        {
            currentState = CookingState.Cooked;
            SetMaterial(cookedMaterial);
        }
        else if (temperature < foodData.burnThresholdTime + 20f)
        {
            currentState = CookingState.Overcooked;
            SetMaterial(overcookedMaterial);
        }
        else
        {
            currentState = CookingState.Burnt;
            SetMaterial(burntMaterial);
        }
    }

    private void SetMaterial(Material mat)
    {
        if (foodRenderer != null && mat != null && foodRenderer.material != mat)
        {
            foodRenderer.material = mat;
        }
    }

    public float GetCookingProgress()
    {
        return Mathf.Clamp01(temperature / foodData.burnThresholdTime);
    }
}
