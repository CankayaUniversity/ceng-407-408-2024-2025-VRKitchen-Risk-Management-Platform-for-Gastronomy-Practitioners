using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILogEntry : UIDialogueEntry, IPointerClickHandler
{
    private Subtitle m_Subtitle;
    private SubtitleManager m_Manager;

    public void Setup(Subtitle subtitle, SubtitleManager manager)
    {
        base.Setup(subtitle);

        m_Subtitle = subtitle;
        m_Manager = manager;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (m_Manager != null)
        {
            m_Manager.ReplaySubtitle(m_Subtitle);
            eventData.Use();
        }
    }
}
