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
    public System.Action onSpeechComplete;


    //private readonly string welcomeMessage = "Welcome to VR: Kitchen Introduction In The Game. Hello. I am your virtual assistant, here to support you throughout your cooking experience. During your time in the kitchen, you may encounter certain risks such as fire hazards, burnt food, or sanitation issues. If you prefer, I can provide a brief overview of these potential hazards and explain how to address them effectively before we begin. This short tutorial may help you navigate the kitchen more confidently and efficiently. Alternatively, if you would rather proceed directly and learn through hands-on experience, we can begin right away. Please indicate whether you would like the risk management overview or wish to start cooking immediately. In the event of a fire, which may occur if the stove is left on without supervision, the player must quickly locate the fire extinguisher, aim at the flames, and spray using a sweeping motion to extinguish the fire safely. If food becomes burnt during the cooking process, it should be discarded immediately, and the player must clean the pan thoroughly with soap and a sponge, rinse and dry it, and then restart the cooking procedure with fresh ingredients. Cross-contamination happens when different raw meats are placed on the same cutting board. To resolve this, the contaminated food should be thrown away, the cutting board must be cleaned with a sponge and water, and the player should wash their hands before continuing. In the case of a water spill on the kitchen floor, the player needs to use a mop to clean the area to prevent slipping hazards and maintain a safe working environment.";

    private void Start()
    {
        if (stopButton == null)
        {
            Debug.LogError("Stop Button is not assigned in the TextToSpeech script.");
            return;
        }

        stopButton.onClick.AddListener(StopSpeaking);

        // 🔊 Speak the welcome message on start
        //Speak(welcomeMessage);
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
            StartCoroutine(FireCallbackAfter(clip.length));

        }
    }

    private IEnumerator FireCallbackAfter(float delay)
{
    yield return new WaitForSeconds(delay);
    onSpeechComplete?.Invoke(); // Call back to whoever is listening
}


    private IEnumerator StopTalkingAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (chefPatrol != null)
            chefPatrol.SetTalkingState(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
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
