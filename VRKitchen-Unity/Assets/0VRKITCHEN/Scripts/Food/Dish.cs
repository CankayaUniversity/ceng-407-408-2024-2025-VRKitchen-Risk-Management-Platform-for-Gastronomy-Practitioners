using UnityEngine;
using System.Collections.Generic;

public class Dish : MonoBehaviour
{
    
    //will be changed later. RAG AI System will provide optimal seasoning amounts for each dish.
    
    private Dictionary<string, int> seasonings = new Dictionary<string, int>();
    
    [SerializeField] private int optimalSalt = 5;
    [SerializeField] private int maxSalt = 8;
    
    [SerializeField] private int optimalPepper = 3;
    [SerializeField] private int maxPepper = 6;
    
    [SerializeField] private int optimalOil = 2;
    [SerializeField] private int maxOil = 4;

    public void AddSeasoning(string seasoningType, int amount)
    {
        if (!seasonings.ContainsKey(seasoningType))
        {
            seasonings[seasoningType] = 0;
        }
        
        seasonings[seasoningType] += amount;
        Debug.Log($"{seasoningType} Level: {seasonings[seasoningType]}");

        CheckSeasoning(seasoningType);
    }

    private void CheckSeasoning(string seasoningType)
    {
        int currentAmount = seasonings[seasoningType];
        int optimalAmount = GetOptimalAmount(seasoningType);
        int maxAmount = GetMaxAmount(seasoningType);
    }

    private int GetOptimalAmount(string seasoningType)
    {
        return seasoningType switch
        {
            "Salt" => optimalSalt,
            "Pepper" => optimalPepper,
            "Oil" => optimalOil,
            _ => 5 
        };
    }

    private int GetMaxAmount(string seasoningType)
    {
        return seasoningType switch
        {
            "Salt" => maxSalt,
            "Pepper" => maxPepper,
            "Oil" => maxOil,
            _ => 10 
        };
    }
}