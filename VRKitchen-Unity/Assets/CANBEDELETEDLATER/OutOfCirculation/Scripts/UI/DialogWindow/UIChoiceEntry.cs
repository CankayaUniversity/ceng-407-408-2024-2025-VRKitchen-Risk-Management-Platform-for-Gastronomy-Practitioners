using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIChoiceEntry : Selectable, ISubmitHandler, IPointerClickHandler, UIComponentInfo.IComponentInfoDataProvider
{
    public Image Background;
    public TextMeshProUGUI ChoiceText;

    private Action m_OnClicked;
    private UIComponentInfo.ComponentInfoData m_Data = new UIComponentInfo.ComponentInfoData();

    public void SetChoice(string choiceText, Action onClicked)
    {
        ChoiceText.text = choiceText;
        m_OnClicked = onClicked;

        m_Data.Name = "Choice: " + choiceText;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
         m_OnClicked();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        m_OnClicked();
    }

    public UIComponentInfo.ComponentInfoData GetComponentInfo()
    {
        return m_Data;
    }
}
