using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITab : Selectable, IPointerClickHandler, ISubmitHandler, UIComponentInfo.IComponentInfoDataProvider
{
    public Action<UITab> OnSelected;

    public TextMeshProUGUI TabName;
    
    public UIScreen ActivatedScreen;
    public Sprite DisabledSprite;

    private Image m_UnderImage;
    Sprite m_DefaultSprite;
    private bool m_Active = false;
    private bool m_Selected = false;

    private UIComponentInfo.ComponentInfoData m_Data = new UIComponentInfo.ComponentInfoData();

    public void Init()
    {
        m_UnderImage = GetComponent<Image>();
        m_DefaultSprite = m_UnderImage.sprite;

        ActivatedScreen.Build();

        m_Data.Name = TabName.text + " tab";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(m_Active) return;
        
        OnSelected.Invoke(this);
    }

    public void Activate()
    {
        m_Active = true;

        ActivatedScreen.Display();
        m_UnderImage.sprite = m_DefaultSprite;
        
        if(ControlManager.CurrentControlType != ControlManager.ControlType.Mouse)
            EventSystem.current.SetSelectedGameObject(gameObject);
        
        UAP_AccessibilityManager.Say(TabName.text + " tab opened", true, true, UAP_AudioQueue.EInterrupt.All);
    }

    //Deactivation callback will be called if the tab is indeed deactivated. This allow to cancel changing tabs for
    //example when we have a modal popup asking if we want to save some setting or not
    public void Deactivate(System.Action deactivationCallback)
    {
        ActivatedScreen.Close(() =>
        {
            m_Active = false;
            m_UnderImage.sprite = DisabledSprite;

            deactivationCallback.Invoke();
        });
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);

        m_Selected = true;
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);

        m_Selected = false;
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if(m_Active) return;
        
        OnSelected.Invoke(this);
    }

    public UIComponentInfo.ComponentInfoData GetComponentInfo()
    {
        return m_Data;
    }
}