using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class UISettingMenu : MonoBehaviour, UIRoot.IUIInitHandler
{
    public static UISettingMenu Instance { get; protected set; }
    
    public RectTransform TabContainer;
    public UIBottomBar BottomBar;
    public UIModalPopup ModalPopup;

    public UITooltipArea TooltipArea;

    public event System.Action OnClosed;

    private UITab[] Tabs;

    private int CurrentTabIndex;
    private UITab CurrentTab;

    private InputAction m_OptionAction;
    
    // Start is called before the first frame update
    void InitData()
    {
        Instance = this;
        
        Tabs = TabContainer.GetComponentsInChildren<UITab>();
        
        foreach (var tab in Tabs)
        {
            tab.Init();
            tab.Deactivate(() => { });
            tab.OnSelected += uiTab =>
            {
                if(uiTab == CurrentTab)
                    return;
                
                CleanScreenToTabNavigation();
                CurrentTab.Deactivate(() =>
                {
                    SetCurrentTab(uiTab);
                });
            };
        }

        var input = ControlManager.CurrentInput;

        input.FindAction("UI/Next tab").performed += context =>
        {
            if (!isActiveAndEnabled) return; //the setting menu isn't on, just ignore that press
            
            int targetIndex = CurrentTabIndex + 1;
            if (targetIndex >= Tabs.Length) targetIndex = 0;
            Tabs[targetIndex].OnSelected.Invoke(Tabs[targetIndex]);
        };
        
        input.FindAction("UI/Previous tab").performed += context =>
        {
            if (!isActiveAndEnabled) return; //the setting menu isn't on, just ignore that press
            
            int targetIndex = CurrentTabIndex - 1;
            if (targetIndex < 0) targetIndex = Tabs.Length - 1;
            Tabs[targetIndex].OnSelected.Invoke(Tabs[targetIndex]);
        };
        
        gameObject.SetActive(false);
        
        UIGameSettingScreen.SwitchAllFont();
    }

    public void Display()
    {
        UISidebar.PushFocusLock();
        
        //we make sure that none of the sidebar had focus, if they did, we exit that focus
        UIToggleSidebar.Instance?.InputLeave();
        
        gameObject.SetActive(true);
        SetCurrentTab(Tabs[0]);
        
        TooltipArea.Activate();
    }

    void SetCurrentTab(UITab tab)
    {
        CurrentTab = tab;
        CurrentTabIndex = Array.FindIndex(Tabs, tab1 => tab1 == CurrentTab);
        
        CurrentTab.Activate();
        
        SetupScreenToTabNavigation();
    }

    public void Close(Action onClose)
    {
        CurrentTab.Deactivate(() =>
        {
            CleanScreenToTabNavigation();
            gameObject.SetActive(false);
            onClose.Invoke();
            
            TooltipArea.Deactivate();
            
            UISidebar.PopFocusLock();
            
            OnClosed?.Invoke();
        });
    }

    public void Init()
    {
        InitData();
        
        var inputReference = ControlManager.CurrentInput;
        m_OptionAction = inputReference.FindAction("Common/Options");
        m_OptionAction.performed += context =>
        {
           ToggleSettingMenu();
        };
    }

    public void ToggleSettingMenu()
    {
        var inputReference = ControlManager.CurrentInput;

        if (!gameObject.activeSelf)
        {
            //enabling it
            Display();
                
            inputReference.FindActionMap("Gameplay").Disable();
            inputReference.FindActionMap("UI").Enable();
        }
        else
        {
            //closing it. Using a callback as a popup can appear asking to save the change so we need to wait for
            //the user decision before actually closing it
                
            //we disable the action until a choice is made, to avoid calling that again
            m_OptionAction.Disable();
                
            Close(() => {
                m_OptionAction.Enable();
                inputReference.FindActionMap("Gameplay").Enable();
                inputReference.FindActionMap("UI").Disable();
            });
        }
    }

    //This make every tab go on move down to the first element on the current screen when opened, and the firstElement
    //of the screen go to its own tab on move up.
    void SetupScreenToTabNavigation()
    {
        var first = CurrentTab.ActivatedScreen.GetFirstControl();

        var nav = first.navigation;
        nav.selectOnUp = CurrentTab;
        first.navigation = nav;
        
        foreach (var tab in Tabs)
        {
            nav = tab.navigation;
            nav.selectOnDown = first;
            tab.navigation = nav;
        }
        
    }

    void CleanScreenToTabNavigation()
    {
        var first = CurrentTab.ActivatedScreen.GetFirstControl();

        var nav = first.navigation;
        nav.selectOnUp = null;
        first.navigation = nav;
        
        foreach (var tab in Tabs)
        {
            nav = tab.navigation;
            nav.selectOnDown = null;
            tab.navigation = nav;
        }
    }
    
}