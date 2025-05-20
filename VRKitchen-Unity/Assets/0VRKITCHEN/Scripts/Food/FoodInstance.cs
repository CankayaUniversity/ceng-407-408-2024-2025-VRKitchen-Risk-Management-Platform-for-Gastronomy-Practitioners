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

    public UnityToAPI toAPI; // ✅ Add this to link RAG query system

    private Renderer foodRenderer;
    private Color originalColor;
    private bool hasSentCookedQuery = false; // ✅ Track query sent state

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
        if (temperature < foodData.requiredTemperature)
        {
            currentState = CookingState.Cooking;
            SetMaterial(rawMaterial);
        }
        else if (temperature < foodData.burnThresholdTime)
        {
            if (currentState != CookingState.Cooked)
            {
                currentState = CookingState.Cooked;
                SetMaterial(cookedMaterial);

                // Send RAG query when first cooked
                if (!hasSentCookedQuery && toAPI != null)
                {
                    toAPI.queryText = "The chicken has been cooked. What now?";
                    toAPI.SubmitQuery();
                    hasSentCookedQuery = true;
                }
            }
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
