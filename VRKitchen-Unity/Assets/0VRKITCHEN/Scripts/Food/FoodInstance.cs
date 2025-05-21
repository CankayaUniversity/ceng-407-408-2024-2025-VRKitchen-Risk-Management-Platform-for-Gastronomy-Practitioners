using System.Collections.Generic;
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

    // Seasoning state
    private Dictionary<string, int> seasonings = new Dictionary<string, int>();
    private bool saltAdded = false;
    private bool pepperAdded = false;
    private bool seasoningQuerySent = false;

    [Header("Seasoning Settings")]
    [SerializeField] private int optimalSalt = 5;
    [SerializeField] private int maxSalt = 8;

    [SerializeField] private int optimalPepper = 3;
    [SerializeField] private int maxPepper = 6;

    [SerializeField] private int optimalOil = 2;
    [SerializeField] private int maxOil = 4;

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

        seasoningQuerySent = false;
        saltAdded = false;
        pepperAdded = false;
        seasonings.Clear();

        SetMaterial(rawMaterial);

        Debug.Log($"[ResetCookingState] {name} → State: {currentState}, Temp: {temperature}");
    }

    public void AddSeasoning(string seasoningType, int amount)
    {
        if (!seasonings.ContainsKey(seasoningType))
            seasonings[seasoningType] = 0;

        seasonings[seasoningType] += amount;
        Debug.Log($"[SEASONING] {name} → {seasoningType}: {seasonings[seasoningType]}");

        // Track salt and pepper flags
        if (seasoningType == "Salt") saltAdded = true;
        if (seasoningType == "Pepper") pepperAdded = true;

        CheckSeasoning(seasoningType);

        // Send query ONCE when both salt and pepper are added
        if (toAPI != null && saltAdded && pepperAdded && !seasoningQuerySent)
        {
            string query = $"I added salt and pepper to the {foodData.foodName.ToLower()}. What now?";
            toAPI.queryText = query;
            toAPI.SubmitQuery();

            seasoningQuerySent = true;
            Debug.Log("[RAG] Sent seasoning query (salt + pepper)");
        }
    }

    public bool HasSeasoning(string type)
    {
        return seasonings.ContainsKey(type) && seasonings[type] > 0;
    }

    private void CheckSeasoning(string seasoningType)
    {
        int current = seasonings[seasoningType];
        int optimal = GetOptimalAmount(seasoningType);
        int max = GetMaxAmount(seasoningType);

        if (current > max)
            Debug.LogWarning($"⚠️ {seasoningType} is overseasoned!");
        else if (current == optimal)
            Debug.Log($"✅ {seasoningType} is perfectly seasoned.");
        else if (current < optimal)
            Debug.Log($"➕ {seasoningType} could use more.");
    }

    private int GetOptimalAmount(string type)
    {
        return type switch
        {
            "Salt" => optimalSalt,
            "Pepper" => optimalPepper,
            "Oil" => optimalOil,
            _ => 5
        };
    }

    private int GetMaxAmount(string type)
    {
        return type switch
        {
            "Salt" => maxSalt,
            "Pepper" => maxPepper,
            "Oil" => maxOil,
            _ => 10
        };
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
    }

    private void UpdateCookingState()
    {
        if (currentState == CookingState.Raw && temperature >= foodData.requiredTemperature)
        {
            currentState = CookingState.Cooked;
            SetMaterial(cookedMaterial);

            if (!hasSentCookedQuery && toAPI != null)
            {
                string seasoningNote = "";
                if (HasSeasoning("Salt")) seasoningNote += " It is salted.";
                if (HasSeasoning("Pepper")) seasoningNote += " It is peppered.";

                toAPI.queryText = $"The {foodData.foodName.ToLower()} has been cooked.{seasoningNote} What now?";
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
