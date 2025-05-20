using UnityEngine;

public enum CookingState
{
    Raw,
    Cooked
}

public class FoodInstance : MonoBehaviour
{
    public FoodData foodData;
    public CookingState currentState = CookingState.Raw;

    public float temperature = 0f;

    public Material rawMaterial;
    public Material cookedMaterial;

    public UnityToAPI toAPI;

    private Renderer foodRenderer;
    private bool hasSentCookedQuery = false;

    private void Awake()
    {
        foodRenderer = GetComponent<Renderer>();
        ResetCookingState();
    }

    public void ResetCookingState()
    {
        temperature = 0f;
        currentState = CookingState.Raw;
        hasSentCookedQuery = false;
        SetMaterial(rawMaterial);

        Debug.Log($"[ResetCookingState] {name} → State: {currentState}, Temp: {temperature}");
    }

    private void Update()
    {
        if (!foodData.isCookable) return;
        UpdateCookingState();
    }

    public void Heat(float amount)
    {
        if (!foodData.isCookable) return;

        temperature += amount;

        // Optional: log temperature in editor
        // Debug.Log($"{name} Temp: {temperature}");
    }

    private void UpdateCookingState()
    {
        if (currentState == CookingState.Raw && temperature >= foodData.requiredTemperature)
        {
            currentState = CookingState.Cooked;
            SetMaterial(cookedMaterial);

            if (!hasSentCookedQuery && toAPI != null)
            {
                toAPI.queryText = $"The {foodData.foodName.ToLower()} has been cooked. What now?";
                toAPI.SubmitQuery();
                hasSentCookedQuery = true;
            }

            Debug.Log($"[COOKED] {name} → Temp: {temperature}");
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
        return Mathf.Clamp01(temperature / foodData.requiredTemperature);
    }
}