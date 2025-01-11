using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; protected set; }

    public AudioSource BGMSource;
    public AudioSource BGAmbienceSource;
    public AudioSource PointSFXSource;

    private struct PlayingSourceData
    {
        public AudioSource Source;
        public float Time;
    }
    private List<PlayingSourceData> m_PlayingAudioSourceBeforeChange = new List<PlayingSourceData>();
    
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        
        //this allow to wait a frame before setting volume. It seems that mixer will ignore set value if we do it JUSt as
        //the app start (which happen e.g. when playing in the editor, as we don't have a start screen) so we wait
        //a single frame before setting volume.
        StartCoroutine(WaitToSetSoundSettings());

        //register to config change, so we can start playing all the source again when we switch from mono to stereo
        AudioSettings.OnAudioConfigurationChanged += changed =>
        {
            //device changed == false mean we Reset the audio config (from changing to mono) so we play again all the
            //source.
            if (!changed && m_PlayingAudioSourceBeforeChange.Count > 0)
            {
                foreach (var source in m_PlayingAudioSourceBeforeChange)
                {
                    source.Source.time = source.Time;
                    source.Source.Play();
                }
            }
        };
        
        InitSystem.RegisterOnInitEvent(() =>
        {
            //we switch to the mode choosen in the settings
            SwitchToSpeakerMode(SaveSystem.CurrentSettings.MonoForced ? AudioSpeakerMode.Mono : AudioSpeakerMode.Stereo);
        });
    }

    IEnumerator WaitToSetSoundSettings()
    {
        yield return null;
        
        var mixer = DataReference.Instance.Mixer;

        string[] settingNames = { "MainVolume", "SFXVolume", "VoiceVolume", "MusicVolume" };
        string[] mixerNames = { "MasterVolume", "SFXVolume", "VoiceVolume", "MusicVolume" };

        for (int i = 0; i < settingNames.Length; i++)
        {
            var setting = SaveSystem.CurrentSettings.GetSoundSetting(settingNames[i]);
            mixer.SetFloat(mixerNames[i], setting.Muted? -80 : Mathf.Log10(Mathf.Max(0.0001f, setting.Volume)) * 30.0f);
        }
    }

    public void SwitchToSpeakerMode(AudioSpeakerMode mode)
    {
        //get all audio source
        m_PlayingAudioSourceBeforeChange.Clear();
        var sources = FindObjectsOfType<AudioSource>();
        foreach (var source in sources)
        {
            if (source.isPlaying)
            {
                m_PlayingAudioSourceBeforeChange.Add(new PlayingSourceData()
                {
                    Source = source,
                    Time = source.time
                });
            }
        }

        var config = AudioSettings.GetConfiguration();
        config.speakerMode = mode;
        AudioSettings.Reset(config);
    }

    public void SetBGMClip(AudioClip clip, AudioClip ambience)
    {
        BGMSource.clip = clip;
        BGMSource.Play();

        BGAmbienceSource.clip = ambience;
        BGAmbienceSource.Play();
    }

    public void PlayPointSFX(AudioClip clip, Vector3 position, bool loop)
    {
        PointSFXSource.transform.position = position;
        PointSFXSource.loop = loop;
        
        if(clip != null)
            PointSFXSource.PlayOneShot(clip);
        else
            PointSFXSource.Stop();
    }
}
