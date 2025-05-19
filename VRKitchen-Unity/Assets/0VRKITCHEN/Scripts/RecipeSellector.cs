using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class RecipeSelector : MonoBehaviour
{
    public TextMeshPro recipeListText;  
    public UnityToAPI unityToAPI;       
    public RecipeManager recipeManager; 

    private void Start()
    {
        recipeListText.text = "Please choose:\n\n[1] Steak and Fries\n[2] Pasta with Tomato Sauce\n[3] Rice and Chicken";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectRecipe("Steak and Fries");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectRecipe("Pasta with Tomato Sauce");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectRecipe("Rice and Chicken");
        }
    }

    public void SelectRecipe(string recipeName)
    {
        Debug.Log($"Selected Recipe: {recipeName}");

        // Reset any previous recipe
        if (recipeManager != null)
            recipeManager.ResetRecipe();

        // Send the structured query to RAG
        string query = $"Give me the JSON for how to make {recipeName} in a cooking game. Return fields: recipe_name, ingredients[], steps[].";
        unityToAPI.queryText = query;
        unityToAPI.SubmitQuery();

        recipeListText.text = "Loading steps for " + recipeName + "...";
    }

    public void UpdateText(string newText)
    {
        recipeListText.text = newText;
    }

    public void DisplaySteps(List<string> steps, int currentStepIndex)
    {
        if (recipeListText == null || steps == null) return;

        recipeListText.text = "Steps:\n";

        for (int i = 0; i < steps.Count; i++)
        {
            if (i == currentStepIndex)
                recipeListText.text += $"<color=yellow>→ {steps[i]}</color>\n";
            else if (i < currentStepIndex)
                recipeListText.text += $"<color=green><s>{steps[i]}</s></color>\n";
            else
                recipeListText.text += $"{i + 1}. {steps[i]}\n";
        }
    }
}
