using UnityEngine;
using UnityEngine.Networking;
using TMPro; // For TextMeshPro UI components
using System.Collections;
public class UnityToAPI : MonoBehaviour
{
    [Header("API Configuration")]
    public string submitQueryUrl = "https://lviubjhdkcolf6ihebsg3aohf40ocbxs.lambda-url.eu-central-1.on.aws/submit_query";
    public string getQueryUrl = "https://lviubjhdkcolf6ihebsg3aohf40ocbxs.lambda-url.eu-central-1.on.aws/get_query";

    [Header("Query Settings")]
    [TextArea] // Allows multi-line text input in the Inspector
    public string queryText; // The question to submit

    [Header("UI Elements")]
    public TMP_Text displayText; // UI Text component to display results

    private string queryId;

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
    private IEnumerator SubmitQueryCoroutine(string queryText)
    {
        QueryData queryData = new QueryData { query_text = queryText };
        string jsonData = JsonUtility.ToJson(queryData);

        UnityWebRequest request = new UnityWebRequest(submitQueryUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Parse the response JSON to extract query_id
            var response = JsonUtility.FromJson<ResponseData>(request.downloadHandler.text);
            queryId = response.query_id; // Store the query_id

            // Display the query_id for debugging purposes
            displayText.text = "Query submitted! Query ID: " + queryId;

            // If a valid query_id is returned, start polling for the result
            if (!string.IsNullOrEmpty(queryId))
            {
                StartCoroutine(PollQueryCoroutine(queryId));
            }
        }
        else
        {
            displayText.text = "Failed to submit query: " + request.error;
        }
    }

    // Coroutine to poll the query status
    private IEnumerator PollQueryCoroutine(string queryId)
    {
        while (true)
        {
            UnityWebRequest request = UnityWebRequest.Get($"{getQueryUrl}?query_id={queryId}");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Log raw response for debugging
                Debug.LogWarning("Raw API Response: " + request.downloadHandler.text);

                var queryResponse = JsonUtility.FromJson<QueryResponseData>(request.downloadHandler.text);

                if (queryResponse.is_complete)
                {
                    // If the query is complete, display the answer
                    if (!string.IsNullOrEmpty(queryResponse.answer_text))
                    {
                        displayText.text = $"Answer: {queryResponse.answer_text}";
                    }
                    else
                    {
                        displayText.text = "Answer: No response received from the server.";
                        Debug.LogWarning("API returned an empty or missing response field.");
                    }
                    yield break;
                }
                else
                {
                    // If not complete, show a processing message
                    displayText.text = "Processing query...";
                }
            }
            else
            {
                string errorMessage = request.downloadHandler.text;
                displayText.text = $"Failed to fetch query status: {request.error}\nDetails: {errorMessage}";
                Debug.LogError($"Error polling query status. HTTP Error: {request.error}\nServer Response: {errorMessage}");
                yield break;
            }

            yield return new WaitForSeconds(3); // Poll every 3 seconds
        }
    }

    // Classes for JSON serialization
    [System.Serializable]
    public class QueryData
    {
        public string query_text; // The query text to send
    }

    [System.Serializable]
    public class ResponseData
    {
        public string query_id; // The query ID returned in the response
    }

    [System.Serializable]
    public class QueryResponseData
    {
        public bool is_complete; // Whether the query is complete
        public string answer_text; // The response to the query
    }
}