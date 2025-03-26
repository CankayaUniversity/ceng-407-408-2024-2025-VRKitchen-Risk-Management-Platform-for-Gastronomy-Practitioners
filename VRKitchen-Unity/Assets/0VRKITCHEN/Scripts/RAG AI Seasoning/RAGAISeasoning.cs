using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RAGAISeasoning : MonoBehaviour
{
    private string apiUrl = "";

    public IEnumerator FetchOptimalSeasonings(string dishName, System.Action<Dictionary<string, int>> callback)
    {
        string url = $"{apiUrl}?dish={dishName}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Dictionary<string, int> seasoningData = ParseSeasoningData(jsonResponse);
                callback?.Invoke(seasoningData);
            }
            else
            {
                Debug.LogError($"Failed to fetch seasonings: {request.error}");
            }
        }
    }

    private Dictionary<string, int> ParseSeasoningData(string jsonResponse)
    {
        return JsonUtility.FromJson<SeasoningResponse>(jsonResponse).seasonings;
    }

    [System.Serializable]
    private class SeasoningResponse
    {
        public Dictionary<string, int> seasonings;
    }
}
