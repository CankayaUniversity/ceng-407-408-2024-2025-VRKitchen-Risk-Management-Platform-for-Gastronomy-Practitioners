using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SettingContainer: ISerializationCallbackReceiver
{
    //Game Settings
    public float UIScale = 1.0f;
    public string IconeSet = "Default";
    
    public bool HighlightInteractiveObject = false;

    public bool UseOpenDyslexicFont;

    public bool PauseDialogue = false;

    public bool UseDynamicCameras = true;
    
    //Display settings
    public FullScreenMode FullScreenMode = FullScreenMode.Windowed;
    public int ResolutionWidth = 1600;
    public int ResolutionHeight = 900;
    public int QualityLevel = 0;
    
    //Sound settings
    [System.Serializable]
    public class SoundSetting
    {
        public float Volume = 0.6f;
        public bool Muted = false;
    }
    
    [SerializeField]
    private List<string> m_SoundSettingKey = new List<string>();
    [SerializeField]
    private List<SoundSetting> m_SoundSettingValues = new List<SoundSetting>();

    private Dictionary<string, SoundSetting> m_SoundSettingLookup = new Dictionary<string, SoundSetting>();

    public bool MonoForced = false;

    // You shouldn't store the return of serialization will recreate new instance, todo: find a better interface fo this?
    public SoundSetting GetSoundSetting(string key)
    {
        SoundSetting ret;
        if (!m_SoundSettingLookup.ContainsKey(key))
        {
            ret = new SoundSetting();
            m_SoundSettingLookup.Add(key, ret);
        }
        else
        {
            ret = m_SoundSettingLookup[key];
        }

        return ret;
    }
    
    // Serialization interfaces
    public void OnBeforeSerialize()
    {
        m_SoundSettingKey.Clear();
        m_SoundSettingValues.Clear(); 
        
        foreach (var pair in m_SoundSettingLookup)
        {
            m_SoundSettingKey.Add(pair.Key);
            m_SoundSettingValues.Add(pair.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        m_SoundSettingLookup.Clear();

        for (int i = 0; i < m_SoundSettingKey.Count; i++)
        {
            m_SoundSettingLookup.Add(m_SoundSettingKey[i], m_SoundSettingValues[i]);
        }
    }
}
