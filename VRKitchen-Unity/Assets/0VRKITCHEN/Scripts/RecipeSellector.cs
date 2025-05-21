using TMPro;
using UnityEngine;

public class RecipeSelector : MonoBehaviour
{
    public TextMeshPro recipeListText; // <-- change this line to TextMeshPro (not TextMeshProUGUI)
    public UnityToAPI unityToAPI;

    private void Start()
    {
        recipeListText.text = "Please choose:\n\n[1] Steak and Potatoes\n[2] Chicken and Potatoes\n[3] Hamburger";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectRecipe("Steak and Potatoes");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectRecipe("Chicken and Potatoes");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectRecipe("Hamburger");
        }
    }

    public void SelectRecipe(string recipeName)
    {
        Debug.Log($"Selected Recipe: {recipeName}");
        string query = $"How to make {recipeName} in the game?";

        unityToAPI.queryText = query;
        unityToAPI.SubmitQuery();

        recipeListText.text = "Loading steps for " + recipeName + "...";
    }

    public void UpdateText(string newText)
    {
        recipeListText.text = newText;
    }
}
