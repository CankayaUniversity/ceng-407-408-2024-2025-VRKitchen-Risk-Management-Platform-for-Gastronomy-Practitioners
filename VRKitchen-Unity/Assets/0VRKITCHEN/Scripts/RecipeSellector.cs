using TMPro;
using UnityEngine;

public class RecipeSelector : MonoBehaviour
{
    public TextMeshPro recipeListText; // <-- change this line to TextMeshPro (not TextMeshProUGUI)
    public UnityToAPI unityToAPI;

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
