using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    private List<string> requiredIngredients = new List<string>();
    private List<string> cookingSteps = new List<string>();
    private List<string> currentIngredients = new List<string>();
    private int currentStepIndex = 0;
    private bool hasSentQuery = false;

    public UnityToAPI toAPI;                    // 👈 Assign in Inspector
    public RecipeSelector recipeSelector;       // 👈 Assign in Inspector

    public void LoadRecipeFromRAG(UnityToAPI.RecipeJSON json)
    {
        requiredIngredients = new List<string>(json.ingredients);
        cookingSteps = new List<string>(json.steps);
        currentIngredients.Clear();
        currentStepIndex = 0;
        hasSentQuery = false;

        Debug.Log("Loaded Recipe: " + json.recipe_name);
        DisplaySteps(); // show steps immediately
    }

    public void AddIngredient(string ingredient)
    {
        if (requiredIngredients == null || requiredIngredients.Count == 0)
            return;

        if (currentStepIndex >= requiredIngredients.Count)
            return;

        string expected = requiredIngredients[currentStepIndex];

        if (ingredient.Equals(expected, System.StringComparison.OrdinalIgnoreCase))
        {
            currentIngredients.Add(ingredient);
            Debug.Log($"✅ Step {currentStepIndex + 1} correct: {ingredient}");

            currentStepIndex++;
            DisplaySteps();

            if (currentStepIndex < cookingSteps.Count)
            {
                if (toAPI != null)
                {
                    toAPI.queryText = $"The player added '{ingredient}'. What’s the next instruction?";
                    toAPI.SubmitQuery();
                }
            }
            else
            {
                Debug.Log("🎉 Recipe completed successfully!");

                if (toAPI != null)
                {
                    toAPI.queryText = "The dish is fully completed. What’s the next step in the game?";
                    toAPI.SubmitQuery();
                }

                hasSentQuery = true;
            }
        }
        else
        {
            Debug.LogWarning($"❌ Wrong ingredient! Expected: {expected}, got: {ingredient}");
        }
    }

    public void ResetRecipe()
    {
        currentIngredients.Clear();
        requiredIngredients.Clear();
        cookingSteps.Clear();
        currentStepIndex = 0;
        hasSentQuery = false;
        DisplaySteps(); // clear UI
    }

    private void DisplaySteps()
    {
        if (recipeSelector != null)
        {
            recipeSelector.DisplaySteps(cookingSteps, currentStepIndex);
        }
    }

    public List<string> GetCookingSteps() => new List<string>(cookingSteps);
    public List<string> GetRequiredIngredients() => new List<string>(requiredIngredients);
    public bool IsRecipeComplete() => hasSentQuery;
}
