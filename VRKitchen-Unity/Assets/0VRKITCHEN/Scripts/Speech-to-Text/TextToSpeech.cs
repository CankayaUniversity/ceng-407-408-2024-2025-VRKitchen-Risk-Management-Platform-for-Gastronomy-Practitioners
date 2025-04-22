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
    [SerializeField] private AudioSource avatarAudioSource;
    [SerializeField] private Button stopButton;
    [SerializeField] private ChefPatrol chefPatrol; // Reference to character's animation controller

    private void Start()
    {
        if (stopButton == null)
        {
            Debug.LogError("Stop Button is not assigned in the TextToSpeech script.");
            return;
        }

        stopButton.onClick.AddListener(StopSpeaking);
    }

    public async void Speak(string text)
    {
        var credentials = new BasicAWSCredentials(
            accessKey: "",
            secretKey: ""
        );
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

            // 🔊 Play audio
            if (audioSource != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }

            if (avatarAudioSource != null)
            {
                avatarAudioSource.clip = clip;
                avatarAudioSource.Play();
            }

            // 🧑‍🍳 Trigger Talking animation
            if (chefPatrol != null)
                chefPatrol.SetTalkingState(true);

            // 🕒 Stop talking after clip ends
            StartCoroutine(StopTalkingAfter(clip.length));
        }
    }

    private IEnumerator StopTalkingAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (chefPatrol != null)
            chefPatrol.SetTalkingState(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            StopSpeaking();
        }
    }

    public void StopSpeaking()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        if (avatarAudioSource != null && avatarAudioSource.isPlaying)
            avatarAudioSource.Stop();

        if (chefPatrol != null)
            chefPatrol.SetTalkingState(false);

        Debug.Log("Speech stopped.");
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
