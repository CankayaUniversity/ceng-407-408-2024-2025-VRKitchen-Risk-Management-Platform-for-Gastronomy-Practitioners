using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundSettingsManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Sliders")]
    public Slider masterSlider;
    public Slider sfxSlider;
    public Slider assistantSlider;

    private void Start()
    {
        LoadSlider(masterSlider, "MasterVolume", "MasterVolume");
        LoadSlider(sfxSlider, "SFXVolume", "SFXVolume");
        LoadSlider(assistantSlider, "AssistantVolume", "AssistantVolume");

        masterSlider.onValueChanged.AddListener(value => SetVolume("MasterVolume", value));
        sfxSlider.onValueChanged.AddListener(value => SetVolume("SFXVolume", value));
        assistantSlider.onValueChanged.AddListener(value => SetVolume("AssistantVolume", value));
    }

    private void SetVolume(string parameterName, float value)
    {
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(parameterName, dB);

        PlayerPrefs.SetFloat(parameterName, value);
        PlayerPrefs.Save();
    }

    private void LoadSlider(Slider slider, string key, string parameterName)
    {
        float value = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : 1.0f;
        slider.value = value;
        SetVolume(parameterName, value); 
    }
}
