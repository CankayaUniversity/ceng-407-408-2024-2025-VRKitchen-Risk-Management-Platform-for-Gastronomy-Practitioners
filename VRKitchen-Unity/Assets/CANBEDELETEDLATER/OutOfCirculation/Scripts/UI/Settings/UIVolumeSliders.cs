using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UIVolumeSliders : MonoBehaviour
{
    public Slider Slider;
    public TextMeshProUGUI ValueDisplay;
    public UIToggleButton MuteButton;

    public string MixerExposedParamterName;
    public AudioMixer Mixer;
    public string SettingKey;

    public Action WasChanged;
    
    public void Setup()
    {
        Slider.onValueChanged.RemoveAllListeners();
        Slider.onValueChanged.AddListener(val =>
        {
            //set the mixer volume
            var setting = SaveSystem.CurrentSettings.GetSoundSetting(SettingKey);
            setting.Volume = val;
            UpdateDisplayedValue();
            UpdateMixer();
            WasChanged.Invoke();
        });

        MuteButton.onClick.RemoveAllListeners();
        MuteButton.onClick.AddListener(() =>
        {
            var setting = SaveSystem.CurrentSettings.GetSoundSetting(SettingKey);
            setting.Muted = !setting.Muted;
            MuteButton.Toggle(setting.Muted);
            UpdateMixer();
            WasChanged.Invoke();
        });

        FindCurrentValue();
    }

    public void FindCurrentValue()
    {
        Slider.SetValueWithoutNotify(SaveSystem.CurrentSettings.GetSoundSetting(SettingKey).Volume);
        MuteButton.Toggle(SaveSystem.CurrentSettings.GetSoundSetting(SettingKey).Muted, false);
        UpdateDisplayedValue();
        UpdateMixer();
    }

    void UpdateDisplayedValue()
    {
        ValueDisplay.text = Mathf.FloorToInt(Slider.value * 100).ToString();
    }

    void UpdateMixer()
    {
        var setting = SaveSystem.CurrentSettings.GetSoundSetting(SettingKey);
        Mixer.SetFloat(MixerExposedParamterName,  setting.Muted ? -80.0f : Mathf.Log10(Mathf.Max(0.0001f, setting.Volume)) * 30.0f);
    }

    public void SaveToSettings()
    {
        SaveSystem.CurrentSettings.GetSoundSetting(SettingKey).Volume = Slider.value;
    }
}
