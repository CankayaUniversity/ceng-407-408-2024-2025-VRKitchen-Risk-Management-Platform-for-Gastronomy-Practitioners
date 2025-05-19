using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "ScriptableObjects/Recipe")]
public class RecipeData : ScriptableObject
{
    public string recipeName;
    [TextArea] public string description;
    public List<string> ingredients;
    [TextArea] public List<string> steps; 
}