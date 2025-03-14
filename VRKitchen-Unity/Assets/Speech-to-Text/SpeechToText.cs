using System.Collections;
using System.IO;
using HuggingFace.API;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SpeechRecognitionTest : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private TextMeshProUGUI recognizedText; // User's speech-to-text result
    [SerializeField] private TextMeshProUGUI apiResponseText; // API-generated answer
    [SerializeField] private TextToSpeech tts; // Reference to TextToSpeech script

    [SerializeField] private TextMeshPro planeText; // Reference to the TextMeshPro component on the textPlane




    private AudioClip clip;
    private byte[] bytes;
    private bool recording;

    private void Start()
    {
        startButton.onClick.AddListener(StartRecording);
        stopButton.onClick.AddListener(StopRecording);
        stopButton.interactable = true;
        startButton.interactable = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            if (recording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }

        if (recording && Microphone.GetPosition(null) >= clip.samples)
        {
            StopRecording();
        }
    }


    private void StartRecording()
    {
        recognizedText.color = Color.white;
        recognizedText.text = "Recording...";
        stopButton.interactable = true;
        clip = Microphone.Start(null, false, 10, 44100);
        recording = true;
    }

    private void StopRecording()
    {
        if (!recording) return;

        recording = false;
        var position = Microphone.GetPosition(null);
        Microphone.End(null);
        var samples = new float[position * clip.channels];
        clip.GetData(samples, 0);
        bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);
        StartCoroutine(AttemptSpeechRecognition(3)); // Retry up to 3 times
    }

    private IEnumerator AttemptSpeechRecognition(int maxRetries)
    {
        int attempts = 0;
        while (attempts < maxRetries)
        {
            attempts++;

            HuggingFaceAPI.AutomaticSpeechRecognition(bytes, response =>
            {
                recognizedText.color = Color.white;
                recognizedText.text = response;
                StartCoroutine(SendQueryToAPI(response));
            }, error =>
            {
                Debug.LogWarning("Speech Recognition Error: " + error);
                if (error.Contains("503"))
                {
                    recognizedText.color = Color.yellow;
                    recognizedText.text = $"Model loading... Retrying ({attempts}/{maxRetries})";
                }
                else
                {
                    recognizedText.color = Color.red;
                    recognizedText.text = "Speech recognition failed!";
                }
            });

            yield return new WaitForSeconds(5); // Wait before retrying

            if (recognizedText.text != "Recording..." && !recognizedText.text.Contains("Retrying"))
            {
                yield break; // Stop retrying if successful
            }
        }

        recognizedText.color = Color.red;
        recognizedText.text = "Speech recognition unavailable!";
    }
    //private string sessionId = "user_123"; // Default session ID, can be dynamically generated or user-specific

    private IEnumerator SendQueryToAPI(string transcribedText)
    {
        apiResponseText.color = Color.yellow;
        apiResponseText.text = "Sending query...";

        string queryUrl = "https://lviubjhdkcolf6ihebsg3aohf40ocbxs.lambda-url.eu-central-1.on.aws/query_with_memory";

        // Create an instance of QueryData
        QueryData data = new QueryData
        {
            query_text = transcribedText,
            session_id = "user_123" // Optional: If your API uses session_id for memory
        };

        string jsonData = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest(queryUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Parse the response
                var result = JsonUtility.FromJson<APIResponse>(request.downloadHandler.text);

                // Display the response on the plane using TextMeshPro
                if (planeText != null)
                {
                    planeText.text = result.response_text; // Update the text on the plane
                }

                // Display the response
                apiResponseText.color = Color.green;
                //apiResponseText.text = "Answer: " + result.response_text;

                // Use Text-to-Speech to speak the response
                tts.Speak(result.response_text);
            }
            else
            {
                Debug.LogError("Query Submission Failed: " + request.downloadHandler.text);
                apiResponseText.color = Color.red;
                apiResponseText.text = "Query submission failed!";
            }
        }
    }
    private byte[] EncodeAsWAV(float[] samples, int frequency, int channels)
    {
        using (var memoryStream = new MemoryStream(44 + samples.Length * 2))
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(frequency);
                writer.Write(frequency * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * 2);

                foreach (var sample in samples)
                {
                    writer.Write((short)(sample * short.MaxValue));
                }
            }
            return memoryStream.ToArray();
        }
    }

    [System.Serializable]
    private class APIResponse
    {
        public string response_text; // The API's response
        public string[] sources;     // Optional: If the API provides sources
        public string session_id;    // Optional: If the API returns a session_id
    }

    [System.Serializable]
    private class QueryDetails
    {
        public string query_id;
        public string query_text;
        public string answer_text;
        public bool is_complete;
    }

    [System.Serializable]
    private class QueryData
    {
        public string query_text;
        public string session_id; // Optional: Only if your API uses session_id
    }

}
