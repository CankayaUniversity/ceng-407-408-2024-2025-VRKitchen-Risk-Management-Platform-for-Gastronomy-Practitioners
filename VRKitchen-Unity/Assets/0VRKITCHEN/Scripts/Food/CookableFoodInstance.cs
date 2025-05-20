using UnityEngine;

public class CookableFoodInstance : MonoBehaviour
{
    public FoodData foodData;
    public CookingState currentState = CookingState.Raw;

    public float temperature = 0f;

    public Material rawMaterial;
    public Material cookedMaterial;
    public Material overcookedMaterial;
    public Material burntMaterial;

    public UnityToAPI toAPI;

    private Renderer foodRenderer;
    private bool hasSentCookedQuery = false;

    private void Awake()
    {
        if (foodData == null || !foodData.isCookable)
        {
            Debug.LogWarning("CookableFoodInstance attached to a non-cookable item.");
            enabled = false;
            return;
        }

        foodRenderer = GetComponent<Renderer>();
        if (foodRenderer != null && rawMaterial != null)
        {
            foodRenderer.material = rawMaterial;
        }
    }

    private void Update()
    {
        Heat(Time.deltaTime); // Example: constant heat (or call externally)
        UpdateCookingState();
    }

    public void Heat(float amount)
    {
        temperature += amount;
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

                if (!hasSentCookedQuery && toAPI != null)
                {
                    toAPI.queryText = $"The {foodData.foodName} has been cooked. What now?";
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
