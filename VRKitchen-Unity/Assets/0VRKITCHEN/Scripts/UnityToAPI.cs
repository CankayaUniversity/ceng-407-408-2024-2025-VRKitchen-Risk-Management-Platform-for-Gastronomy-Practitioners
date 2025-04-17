using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class UnityToAPI : MonoBehaviour
{
    public string queryUrl;
    [TextArea] public string queryText;

    public TextMeshPro planeText; // This is still okay if you want to assign it manually
    public HandTextDisplay textDisplay; // << Assign this in inspector

    private string sessionId;

    [ContextMenu("Submit Query")]
    public void SubmitQuery()
    {
        if (!string.IsNullOrEmpty(queryText))
        {
            StartCoroutine(SubmitQueryCoroutine(queryText));
        }
    }

    IEnumerator SubmitQueryCoroutine(string queryText)
    {
        QueryData query = new QueryData { query_text = queryText, session_id = sessionId };
        string jsonData = JsonUtility.ToJson(query);

        UnityWebRequest req = new UnityWebRequest(queryUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<ResponseData>(req.downloadHandler.text);
            sessionId = response.session_id;

            if (textDisplay != null)
                textDisplay.DisplayResponseText(response.response_text);
            else if (planeText != null)
                planeText.text = response.response_text;
        }
        else
        {
            string err = "API ERROR: " + req.error;
            if (textDisplay != null)
                textDisplay.DisplayResponseText(err);
            else if (planeText != null)
                planeText.text = err;
        }
    }

    [System.Serializable]
    public class QueryData
    {
        public string query_text;
        public string session_id;
    }

    [System.Serializable]
    public class ResponseData
    {
        public string session_id;
        public string response_text;
        public string[] sources;
    }
}
