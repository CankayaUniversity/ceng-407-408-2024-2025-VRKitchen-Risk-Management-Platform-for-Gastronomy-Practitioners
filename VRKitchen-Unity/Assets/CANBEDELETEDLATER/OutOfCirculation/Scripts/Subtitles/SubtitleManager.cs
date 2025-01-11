using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class SubtitleManager : MonoBehaviour
{
    public AudioSource ReplayAudioSource;

    List<Subtitle> m_CurrentSubtitles = new List<Subtitle>(1);

    List<Subtitle> m_SubtitleLog = new List<Subtitle>();

    public void AddToCurrentSubtitles(Subtitle subtitle)
    {
        if(m_CurrentSubtitles.Contains(subtitle))
            return;
     
        //TODO : move that to a dialogue manager? It overlaps a lot with the log though, so probably better to handle
        //it in the same place?
        //Also need to check if running otherwise scrubbing the thing in editor will add entry....
        if(Application.isPlaying)
            UIDialogueWindow.Instance.AddDialogueLine(subtitle);
        
        m_CurrentSubtitles.Add(subtitle);

        if (m_CurrentSubtitles.Count > 1)
        {
            m_CurrentSubtitles.Sort();
            m_CurrentSubtitles.Reverse();
        }
        
        UpdateDisplayedSubtitle();
    }

    public void RemoveFromCurrentSubtitles(Subtitle subtitle)
    {
        if(!m_CurrentSubtitles.Contains(subtitle))
            return;
        
        m_CurrentSubtitles.Remove(subtitle);
        
        if(subtitle.IsLogged)
            AddSubtitleToLog(subtitle);
        
        UpdateDisplayedSubtitle();
    }

    void UpdateDisplayedSubtitle()
    {
        
    }

    void AddSubtitleToLog(Subtitle subtitle)
    {
        m_SubtitleLog.Add(subtitle);
        
        UILog.Instance.AddLogEntry(subtitle, this);
    }

    public void ReplaySubtitle(Subtitle subtitle)
    {
        ReplayAudioSource.PlayOneShot(subtitle.AudioClip);
    }

#if UNITY_EDITOR
    public static void GatherProperties(SubtitleTrack subtitleTrack, PlayableDirector director, IPropertyCollector driver)
    {
        SubtitleManager subtitleManager = director.GetGenericBinding(subtitleTrack) as SubtitleManager;
        // driver.AddFromName(subtitleManager.textComponent, "m_text");
        // driver.AddFromName(subtitleManager.textComponent, "m_fontColor");
        // driver.AddFromName(subtitleManager.speakerIcon, "m_Sprite");
    }
#endif
}
