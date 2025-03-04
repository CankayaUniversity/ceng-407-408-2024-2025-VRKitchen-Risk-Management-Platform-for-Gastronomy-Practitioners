using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.Runtime;
using UnityEngine;
using UnityEngine.Networking;

public class TextToSpeech : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    public async void Speak(string text)
    {
        var credentials = new BasicAWSCredentials(accessKey:"",secretKey: "");
        var client = new AmazonPollyClient(credentials, Amazon.RegionEndpoint.EUCentral1);

        var request = new SynthesizeSpeechRequest(){
            Text = text,
            Engine = Engine.Neural,
            VoiceId = VoiceId.Joanna,
            OutputFormat = OutputFormat.Mp3
        };
        
        var response = await client.SynthesizeSpeechAsync(request);
        WriteIntoFile(response.AudioStream);

        using(var www = UnityWebRequestMultimedia.GetAudioClip($"{Application.persistentDataPath}/audio.mp3", AudioType.MPEG))
        {
            var op = www.SendWebRequest();

            while (!op.isDone) await Task.Yield();

            var clip = DownloadHandlerAudioClip.GetContent(www);

            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    private void WriteIntoFile(Stream stream)
    {
        using(var fileStream = new FileStream($"{Application.persistentDataPath}/audio.mp3", FileMode.Create))
        {
            byte[] buffer = new byte[8*1024];
            int bytesRead;
            while((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0 )
            {
                fileStream.Write(buffer,0,bytesRead); 
            }
        }
    }

}
