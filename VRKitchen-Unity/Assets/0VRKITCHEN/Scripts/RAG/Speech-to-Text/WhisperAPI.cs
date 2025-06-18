using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public static class WhisperAPI
{
    private const string endpoint = "https://api-inference.huggingface.co/models/openai/whisper-large-v3";
    private const string token = ""; //  Replace with your actual token

    public static IEnumerator Transcribe(byte[] wavData, Action<string> onSuccess, Action<string> onError)
    {
        using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(wavData);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Authorization", token);
            request.SetRequestHeader("Content-Type", "audio/wav");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string json = request.downloadHandler.text;
                    var result = JsonUtility.FromJson<Response>(json);
                    onSuccess?.Invoke(result.text);
                }
                catch (Exception ex)
                {
                    onError?.Invoke("Failed to parse response: " + ex.Message);
                }
            }
            else
            {
                onError?.Invoke($"API Error {request.responseCode}: {request.downloadHandler.text}");
            }
        }
    }

    [Serializable]
    private class Response
    {
        public string text;
    }
}
