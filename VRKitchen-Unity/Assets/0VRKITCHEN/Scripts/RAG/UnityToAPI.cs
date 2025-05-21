using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class UnityToAPI : MonoBehaviour
{
    public string queryUrl;
    [TextArea] public string queryText;

    public TextMeshPro planeText; // This is still okay if you want to assign it manually
    public HandTextDisplay textDisplay; // << Assign this in     
    public TextToSpeech tts; // Add this and assign it in Inspector

    private bool isQueryInProgress = false;
    private float cooldownTime = 2f; // ⏳ 2-second cooldown
    private float lastQueryTime = -10f; // Track last query time



    private string sessionId;

    [ContextMenu("Submit Query")]
    [ContextMenu("Submit Query")]
    public void SubmitQuery()
    {
        // Don’t allow multiple queries or spamming
        if (isQueryInProgress || Time.time - lastQueryTime < cooldownTime || string.IsNullOrEmpty(queryText))
            return;

        StartCoroutine(SubmitQueryCoroutine(queryText));
    }


    IEnumerator SubmitQueryCoroutine(string queryText)
    {

        isQueryInProgress = true;
        lastQueryTime = Time.time; // Start cooldown timer


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

            string reply = response.response_text;

            //Display response
            if (textDisplay != null)
                textDisplay.DisplayResponseText(reply);
            else if (planeText != null)
                planeText.text = reply;

            // Stop current speech and speak new response
            if (tts != null)
            {
                tts.StopSpeaking();
                if (!string.IsNullOrEmpty(reply))
                    tts.Speak(reply);
            }


        }
        else
        {
            string err = "API ERROR: " + req.error;
            if (textDisplay != null)
                textDisplay.DisplayResponseText(err);
            else if (planeText != null)
                planeText.text = err;

            // Stop & Speak error message
            if (tts != null)
            {
                tts.StopSpeaking();
                tts.Speak("Sorry, there was an error contacting the assistant.");
            }
        }

        isQueryInProgress = false; // Unlock after completion

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