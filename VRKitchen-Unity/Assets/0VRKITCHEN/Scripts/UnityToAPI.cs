using UnityEngine;
using UnityEngine.Networking;
using TMPro; // For TextMeshPro UI components
using System.Collections;

public class UnityToAPI : MonoBehaviour
{
    [Header("API Configuration")]
    public string queryUrl = "https://lviubjhdkcolf6ihebsg3aohf40ocbxs.lambda-url.eu-central-1.on.aws/query_with_memory";

    [Header("Query Settings")]
    [TextArea] // Allows multi-line text input in the Inspector
    public string queryText; // The question to submit

    [Header("UI Elements")]
    public TMP_Text displayText; // UI Text component to display results

    private string sessionId; // To maintain context between queries

    // Trigger query submission from Inspector
    [ContextMenu("Submit Query")] // Adds a right-click option in the Inspector
    public void SubmitQuery()
    {
        if (!string.IsNullOrEmpty(queryText))
        {
            StartCoroutine(SubmitQueryCoroutine(queryText));
        }
        else
        {
            Debug.LogWarning("Query text is empty! Please enter a question in the Inspector.");
            if (displayText != null)
            {
                displayText.text = "Please enter a question in the Inspector.";
            }
        }
    }

    // Coroutine to submit the query
    public IEnumerator SubmitQueryCoroutine(string queryText)
    {
        // Create the query data with session_id (if available)
        QueryData queryData = new QueryData
        {
            query_text = queryText,
            session_id = sessionId // Use the existing session_id if available
        };

        string jsonData = JsonUtility.ToJson(queryData);

        UnityWebRequest request = new UnityWebRequest(queryUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Parse the response JSON
            var response = JsonUtility.FromJson<ResponseData>(request.downloadHandler.text);

            // Update the session_id for future queries
            sessionId = response.session_id;

            // Display the response
            displayText.text = $"Response: {response.response_text}\nSources: {string.Join(", ", response.sources)}";
        }
        else
        {
            displayText.text = "Failed to submit query: " + request.error;
        }
    }

    // Classes for JSON serialization
    [System.Serializable]
    public class QueryData
    {
        public string query_text; // The query text to send
        public string session_id; // The session ID for context (optional)
    }

    [System.Serializable]
    public class ResponseData
    {
        public string session_id; // The session ID returned in the response
        public string response_text; // The response text
        public string[] sources; // The sources for the response
    }
}