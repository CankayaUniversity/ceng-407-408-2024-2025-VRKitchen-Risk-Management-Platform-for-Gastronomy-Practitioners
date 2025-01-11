using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILog : MonoBehaviour, UIRoot.IUIInitHandler
{
    public static UILog Instance { get; private set; }
    
    public UILogEntry LogEntryPrefab;
    public UILogChoiceEntry ChoiceEntryPrefab;
    public RectTransform ContentParent;


    private List<UILogEntry> m_EntriesToScale = new List<UILogEntry>();
    private List<UILogChoiceEntry> m_ChoiceEntriesToScale = new List<UILogChoiceEntry>();

    public void Init()
    {
        Instance = this;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        
        UAP_AccessibilityManager.Say("Log Window Open");
        
        //we rescale all the log entry that where added since last time we opened the log
        //the log need to be enabled (i.e.g opened) for the layout system to scale horizontally the log entry, so we wait
        //until we open it to do it. We also need to wait 1 frame so the layout have set the right value, so we start a 
        //coroutine that will wait a frame after opening before resizing all the new entries
        StartCoroutine(DelayResizing());
    }

    IEnumerator DelayResizing()
    {
        yield return new WaitForEndOfFrame();
        
        foreach (var entry in m_EntriesToScale)
        {
            //width is the content log size minus the icon picture size
            float width = ContentParent.rect.width -  entry.IconeContainer.rect.width;
            float targetHeight = entry.SubtitleEntry.GetPreferredValues(entry.SubtitleEntry.text, width, 0).y;
            
            targetHeight = Mathf.Max(targetHeight, entry.IconeContainer.rect.height);

            entry.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
        }

        foreach (var choiceEntry in m_ChoiceEntriesToScale)
        {
            //width is the content log size minus the icon picture size
            float targetHeight = choiceEntry.EntryText.GetPreferredValues(choiceEntry.EntryText.text, ContentParent.rect.width, 0).y;
            choiceEntry.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);     
        }
        
        m_EntriesToScale.Clear();
        m_ChoiceEntriesToScale.Clear();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        UAP_AccessibilityManager.Say("Log Window Closed");
    }

    public void AddLogEntry(Subtitle subtitle, SubtitleManager manager)
    {
        UILogEntry logEntryInstance = Instantiate(LogEntryPrefab, ContentParent, false);
        UIGameSettingScreen.SwitchFontFor(logEntryInstance.GetComponentsInChildren<TextMeshProUGUI>());
        
        logEntryInstance.Setup(subtitle, manager);
        
        m_EntriesToScale.Add(logEntryInstance);
    }

    public void AddChoice(string pickedChoice)
    {
        UILogChoiceEntry choiceEntryPrefab = Instantiate(ChoiceEntryPrefab, ContentParent, false);
        UIGameSettingScreen.SwitchFontFor(choiceEntryPrefab.GetComponentsInChildren<TextMeshProUGUI>());
        choiceEntryPrefab.EntryText.text = $"[{pickedChoice}]";
        
        m_ChoiceEntriesToScale.Add(choiceEntryPrefab);
    }
}
