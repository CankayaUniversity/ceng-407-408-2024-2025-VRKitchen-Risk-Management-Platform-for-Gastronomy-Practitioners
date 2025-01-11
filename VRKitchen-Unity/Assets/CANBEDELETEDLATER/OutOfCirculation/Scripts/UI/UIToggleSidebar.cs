using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIToggleSidebar : UISidebar, IPointerEnterHandler, IPointerExitHandler
{
    public static UIToggleSidebar Instance { get; private set; }

    public Button SettingsButton;
    
    [Header("Accessibility Toggles")]
    public Button MaxZoomToggle;
    public Button PauseDialogueToggle;

    [Header("Log")] 
    public Button LogSwitchButton;
    public UILog Log;

    [Header("UI References")] 
    public UITooltipPopup TooltipPopup;
    
    private Color m_NormalColor;
    private Color m_ToggledColor;

    private void Awake()
    {
        Instance = this;
        
        //we're reading saved value so we make sure to get init only after the save system have load the settings
        InitSystem.RegisterOnInitEvent(InitToggleBar);
        
        TooltipPopup.gameObject.SetActive(false);
    }

    void InitToggleBar()
    {
        m_NormalColor = Color.white;
        m_ToggledColor = Color.grey;

        MaxZoomToggle.onClick.AddListener(() =>
        {

            bool zoomed = SaveSystem.CurrentSettings.UIScale > 1.05f;

            if (!zoomed)
            {
                SaveSystem.CurrentSettings.UIScale = 1.5f;
                VisualManager.RescaleUIToValue(1.5f);
                SaveSystem.SaveSetting();
                
                UAP_AccessibilityManager.Say("UI Zoom On");
            }
            else
            {
                SaveSystem.CurrentSettings.UIScale = 1.0f;
                VisualManager.RescaleUIToValue(1.0f);
                SaveSystem.SaveSetting();
                UAP_AccessibilityManager.Say("UI Zoom Off");
            }

            UpdateState();
        });

        //Pause dialog
        PauseDialogueToggle.onClick.AddListener(() =>
        {
            SaveSystem.CurrentSettings.PauseDialogue = !SaveSystem.CurrentSettings.PauseDialogue;
            SaveSystem.SaveSetting();

            UAP_AccessibilityManager.Say("Dialogue Pause " + (SaveSystem.CurrentSettings.PauseDialogue ? "On" :  "Off"));
            
            UpdateState();
        });


        UpdateState();

        //Log

        LogSwitchButton.onClick.AddListener(() =>
        {
            if (Log.gameObject.activeSelf)
            {
                Log.Hide();
            }
            else
            {
                Log.Show();
            }
        });

        //Init input
        RegisterFocus(ControlManager.CurrentInput.FindAction("Common/AccessLeftSidebar"), SettingsButton.gameObject);
    }

    protected override void Focused()
    {
        UAP_AccessibilityManager.Say("Option Sidebar Focused", false);
        TooltipPopup.Activate();
    }

    protected override void Unfocused()
    {
        TooltipPopup.Deactivate();
    }

    /// <summary>
    /// Called by stuff that modify setting to update the state of buttons
    /// </summary>
    public void UpdateState()
    {
        //TODO : Replace that with special button toggle so we can handle their state change instead of costly query and #
        //bad looking color change
        MaxZoomToggle.GetComponent<Image>().color =  SaveSystem.CurrentSettings.UIScale > 1.05f ? m_ToggledColor : m_NormalColor;
        PauseDialogueToggle.GetComponent<Image>().color = SaveSystem.CurrentSettings.PauseDialogue ? m_ToggledColor : m_NormalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipPopup.Activate();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipPopup.Deactivate();
    }
}
