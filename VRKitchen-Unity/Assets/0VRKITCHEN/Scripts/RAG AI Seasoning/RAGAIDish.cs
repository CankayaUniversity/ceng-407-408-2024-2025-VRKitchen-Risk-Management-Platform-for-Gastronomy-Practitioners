using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RAGAIDish : MonoBehaviour
{
    private Dictionary<string, int> seasonings = new Dictionary<string, int>();
    private Dictionary<string, int> optimalSeasonings = new Dictionary<string, int>();

    private RAGAISeasoning ragAI;

    private void Start()
    {
        ragAI = FindObjectOfType<RAGAISeasoning>();
        StartCoroutine(ragAI.FetchOptimalSeasonings(gameObject.name, UpdateOptimalSeasonings));
    }

    private void UpdateOptimalSeasonings(Dictionary<string, int> aiSeasonings)
    {
        optimalSeasonings = aiSeasonings;
        //Debug.Log("Optimal Seasoning Levels Updated from RAG AI!");
    }

    public void AddSeasoning(string seasoningType, int amount)
    {
        if (!seasonings.ContainsKey(seasoningType))
        {
            seasonings[seasoningType] = 0;
        }
        
        seasonings[seasoningType] += amount;
        //Debug.Log($"{seasoningType} Level: {seasonings[seasoningType]}");

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
        return optimalSeasonings.ContainsKey(seasoningType) ? optimalSeasonings[seasoningType] : 5;
    }

    private int GetMaxAmount(string seasoningType)
    {
        return optimalSeasonings.ContainsKey(seasoningType) ? optimalSeasonings[seasoningType] + 3 : 10;
    }
}
