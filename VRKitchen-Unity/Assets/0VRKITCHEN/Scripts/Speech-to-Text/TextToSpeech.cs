using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.Runtime;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI; 

public class TextToSpeech : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Button stopButton; // Reference to the stop button

    private void Start()
    {
        // Ensure the stopButton is assigned
        if (stopButton == null)
        {
            Debug.LogError("Stop Button is not assigned in the TextToSpeech script.");
            return;
        }

        // Link the stop button to the StopSpeaking method
        stopButton.onClick.AddListener(StopSpeaking);
    }

    public async void Speak(string text)
    {
        var credentials = new BasicAWSCredentials(accessKey: "", secretKey: "");
        var client = new AmazonPollyClient(credentials, Amazon.RegionEndpoint.EUCentral1);

        var request = new SynthesizeSpeechRequest()
        {
            Text = text,
            Engine = Engine.Neural,
            VoiceId = VoiceId.Joanna,
            OutputFormat = OutputFormat.Mp3
        };

        var response = await client.SynthesizeSpeechAsync(request);
        WriteIntoFile(response.AudioStream);

        using (var www = UnityWebRequestMultimedia.GetAudioClip($"{Application.persistentDataPath}/audio.mp3", AudioType.MPEG))
        {
            var op = www.SendWebRequest();

            while (!op.isDone) await Task.Yield();

            var clip = DownloadHandlerAudioClip.GetContent(www);

            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    // Method to stop speaking
    public void StopSpeaking()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop(); // Stop the audio playback
            Debug.Log("Speech stopped.");
        }
    }

    private void WriteIntoFile(Stream stream)
    {
        using (var fileStream = new FileStream($"{Application.persistentDataPath}/audio.mp3", FileMode.Create))
        {
            byte[] buffer = new byte[8 * 1024];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
            }
        }
    }
}