using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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

    public InputActionReference RecordSpeech;

    private void Start()
    {
        startButton.onClick.AddListener(StartRecording);
        stopButton.onClick.AddListener(StopRecording);
        stopButton.interactable = true;
        startButton.interactable = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) || RecordSpeech.action.triggered)
        {
            if (recording)
                StopRecording();
            else
                StartRecording();
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

        clip = Microphone.Start(null, false, 10, 16000); // 16kHz mono
        recording = true;
    }

    private void StopRecording()
    {
        if (!recording) return;

        recording = false;
        int position = Microphone.GetPosition(null);
        Microphone.End(null);

        float[] samples = new float[position * clip.channels];
        clip.GetData(samples, 0);

        bytes = EncodeAsWAV(samples, 16000, 1);
        StartCoroutine(AttemptSpeechRecognition(3));
    }

    private IEnumerator AttemptSpeechRecognition(int maxRetries)
    {
        int attempts = 0;

        while (attempts < maxRetries)
        {
            attempts++;

            yield return WhisperAPI.Transcribe(
                bytes,
                response =>
                {
                    recognizedText.color = Color.white;
                    recognizedText.text = response;
                    StartCoroutine(SendQueryToAPI(response));
                },
                error =>
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
                }
            );

            if (!recognizedText.text.Contains("Retrying") && recognizedText.text != "Recording...")
                yield break;

            yield return new WaitForSeconds(5);
        }

        recognizedText.color = Color.red;
        recognizedText.text = "Speech recognition unavailable!";
    }

    private IEnumerator SendQueryToAPI(string transcribedText)
    {
        apiResponseText.color = Color.yellow;
        apiResponseText.text = "Sending query...";

        string queryUrl = "https://lviubjhdkcolf6ihebsg3aohf40ocbxs.lambda-url.eu-central-1.on.aws/query_with_memory";

        QueryData data = new QueryData
        {
            query_text = transcribedText + " without extra explanation. Just give me the answer.",
            session_id = "default"
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
                var result = JsonUtility.FromJson<APIResponse>(request.downloadHandler.text);

                if (planeText != null)
                    planeText.text = result.response_text;

                apiResponseText.color = Color.green;
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
        using (var writer = new BinaryWriter(memoryStream))
        {
            writer.Write("RIFF".ToCharArray());
            writer.Write(36 + samples.Length * 2);
            writer.Write("WAVE".ToCharArray());
            writer.Write("fmt ".ToCharArray());
            writer.Write(16);
            writer.Write((ushort)1); // PCM format
            writer.Write((ushort)channels);
            writer.Write(frequency);
            writer.Write(frequency * channels * 2);
            writer.Write((ushort)(channels * 2));
            writer.Write((ushort)16); // 16 bits per sample
            writer.Write("data".ToCharArray());
            writer.Write(samples.Length * 2);

            foreach (var sample in samples)
                writer.Write((short)(sample * short.MaxValue));

            return memoryStream.ToArray();
        }
    }

    [System.Serializable]
    private class APIResponse
    {
        public string response_text;
        public string[] sources;
        public string session_id;
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
        public string session_id;
    }
}
