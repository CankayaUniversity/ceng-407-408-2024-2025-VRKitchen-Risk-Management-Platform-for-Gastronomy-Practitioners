using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIStartScreen : MonoBehaviour
{
    public GameObject StartPopup;
    public Button FirstButton;

    private bool m_Focused = false;

    private void Awake()
    {
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
    }

    private void Start()
    {
        DataReference.Instance.CreateUI();

        m_Focused = false;
        StartPopup.gameObject.SetActive(true);

        ControlManager.OnControlTypeChanged += HandleControlChange;
    }

    public void BackToMenu()
    {
        m_Focused = true;
        HandleControlChange();
    }

    void HandleControlChange()
    {
        if (m_Focused && ControlManager.CurrentControlType != ControlManager.ControlType.Mouse)
        {
            EventSystem.current.SetSelectedGameObject(FirstButton.gameObject);
        }
    }
    
    public void StartNewGame()
    {
        ControlManager.OnControlTypeChanged -= HandleControlChange;
        
        UIRoot.Instance.EnableGameUI(true);
        InitSystem.SpawnGameplayData();
        SpawnSystem.LoadInScene(2,2);
    }

    public void OpenSettings()
    {
        m_Focused = false;
        UISettingMenu.Instance.Display();
        UISettingMenu.Instance.OnClosed += SettingMenuClosed;
    }

    void SettingMenuClosed()
    {
        UISettingMenu.Instance.OnClosed -= SettingMenuClosed;
        BackToMenu();
    }
    
    public void ExitGame()
    {
        Application.Quit();
    }
}
