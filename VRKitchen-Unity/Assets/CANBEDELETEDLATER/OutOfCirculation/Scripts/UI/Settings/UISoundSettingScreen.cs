using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISoundSettingScreen : UIScreen
{
    public UIVolumeSliders MasterVolume;
    public UIVolumeSliders SFXVolume;
    public UIVolumeSliders VoiceVolume;
    public UIVolumeSliders MusicVolume;

    public Toggle ForceMonoToggle;

    public override void Build()
    {
        base.Build();

        MasterVolume.Setup();
        MasterVolume.WasChanged += () =>
        {
            Dirty();
        };

        SFXVolume.Setup();
        SFXVolume.WasChanged += () =>
        {
            Dirty();
        };
        
        VoiceVolume.Setup();
        VoiceVolume.WasChanged += () =>
        {
            Dirty();
        };
        
        MusicVolume.Setup();
        MusicVolume.WasChanged += () =>
        {
            Dirty();
        };
        
        ForceMonoToggle.SetIsOnWithoutNotify(SaveSystem.CurrentSettings.MonoForced);
        ForceMonoToggle.onValueChanged.AddListener(newValue =>
        {
            SaveSystem.CurrentSettings.MonoForced = newValue;
            AudioManager.Instance.SwitchToSpeakerMode(newValue ? AudioSpeakerMode.Mono : AudioSpeakerMode.Stereo);
            Dirty();
        });
    }

    protected override void SetupKeyInput()
    {
        EventSystem.current.SetSelectedGameObject(MasterVolume.Slider.gameObject);
    }

    protected override void SaveChange(Action onSave)
    {
        MasterVolume.SaveToSettings();
        SFXVolume.SaveToSettings();
        VoiceVolume.SaveToSettings();
        MusicVolume.SaveToSettings();
        
        base.SaveChange(onSave);
    }

    void FindCurrentValue()
    {
        MasterVolume.FindCurrentValue();
        SFXVolume.FindCurrentValue();
        VoiceVolume.FindCurrentValue();
        MusicVolume.FindCurrentValue();
        
        ForceMonoToggle.SetIsOnWithoutNotify(SaveSystem.CurrentSettings.MonoForced);
    }

    protected override void UndoChange()
    {
        bool mono = SaveSystem.CurrentSettings.MonoForced;
        
        base.UndoChange();
        
        FindCurrentValue();
        
        //we only switched speaker mode if it have change, as for now, switching created an audible pop.
        //TODO : try to fix that pop
        if(mono != SaveSystem.CurrentSettings.MonoForced)
            AudioManager.Instance.SwitchToSpeakerMode(SaveSystem.CurrentSettings.MonoForced ? AudioSpeakerMode.Mono : AudioSpeakerMode.Stereo);
    }

    public override Selectable GetLastControl()
    {
        return ForceMonoToggle;
    }

    public override Selectable GetFirstControl()
    {
        return MasterVolume.Slider;
    }
}
