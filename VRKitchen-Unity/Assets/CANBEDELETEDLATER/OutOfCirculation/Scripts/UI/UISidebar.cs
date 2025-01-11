using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Base class of the sidebars. Handle focusing/defocusing them on a given input
/// </summary>
public abstract class UISidebar : MonoBehaviour
{
    //thing like opening a menu or a window will increment this.
    //It will be decremented when they close. If at any point this is > 0 sidebar can't be focused
    private static int s_FocusLock = 0;
    
    public TextMeshProUGUI InputPromptText;
    public Image InputPromptImage;
    
    //only one sidebar at all time can have input focus
    private static UISidebar s_HasFocus = null;
    
    protected bool m_HasInputFocus;
    protected GameObject m_FirstFocus;

    protected InputAction m_FocusAction;
    
    protected void RegisterFocus(InputAction focusAction, GameObject firstFocus)
    {
        focusAction.performed += context =>
        {
            //we do not focus if focus action was disable (e.g. dialogue or menu opened)
            if(s_FocusLock > 0)
                return;
            
            InputFocus();
        };

        m_FirstFocus = firstFocus;
        m_FocusAction = focusAction;

        ControlManager.OnControlTypeChanged += HandleInputMethodChange;
        HandleInputMethodChange();
    }

    void InputFocus()
    {
        if(m_HasInputFocus)
            return;
        
        if(s_HasFocus != null)
            s_HasFocus.InputLeave();
        
        m_HasInputFocus = true;
        EnableLeavingInput(true);
        
        ControlManager.SwitchToUI();
        
        EventSystem.current.SetSelectedGameObject(m_FirstFocus);

        s_HasFocus = this;
        
        UpdateInputPrompt();
        
        Focused();
    }
    
    protected virtual void Focused() { }
    protected virtual void Unfocused() { }

    public void InputLeave()
    {
        if (!m_HasInputFocus) return;
        
        Debug.Log($"Leaving input focus from {name}");

        ControlManager.SwitchToGameplay();
        
        //we don't want cancel in other screen later to callback this sidebar, so remove the callback
        EnableLeavingInput(false);
        EventSystem.current.SetSelectedGameObject(null);
        
        m_HasInputFocus = false;
        s_HasFocus = null;
        
        Unfocused();

        UpdateInputPrompt();
    }

    void InputFocusLeaveCallback(InputAction.CallbackContext context)
    {
        InputLeave();
        UAP_AccessibilityManager.Say("Game window focused", false, true, UAP_AudioQueue.EInterrupt.All);
    }

    void HandleInputMethodChange()
    {
        if (ControlManager.CurrentControlType == ControlManager.ControlType.Mouse)
        {
            InputPromptImage.transform.parent.gameObject.SetActive(false);
            InputLeave();
        }
        else
        {
            
            InputPromptImage.transform.parent.gameObject.SetActive(true);
            UpdateInputPrompt();
        }
    }

    public void EnableLeavingInput(bool enabled)
    {
        //if the sidebar don't have input focus, we don't want to do any of that, so just ignore
        
        if (m_HasInputFocus && enabled)
        {
            ControlManager.UICancelAction.performed += InputFocusLeaveCallback;
        }
        else if(m_HasInputFocus)
        {
            ControlManager.UICancelAction.performed -= InputFocusLeaveCallback;
        }
    }

    void UpdateInputPrompt()
    {
        if (ControlManager.CurrentControlType == ControlManager.ControlType.Mouse)
        {
            InputPromptImage.transform.parent.gameObject.SetActive(false);
            return;
        }
        
        ControlIconSelector.Mapping mapping = null;
        string inputName = "";
        
        if (!m_HasInputFocus)
        {
            UIHelpers.GetInputTextOrIcon(m_FocusAction, ControlManager.CurrentControlScheme.bindingGroup,
                out mapping, out inputName);
        }
        else
        {
            UIHelpers.GetInputTextOrIcon(ControlManager.UICancelAction, ControlManager.CurrentControlScheme.bindingGroup,
                out mapping, out inputName);
        }

        if (mapping != null)
        {
            InputPromptText.gameObject.SetActive(false);
            InputPromptImage.gameObject.SetActive(true);
                
            InputPromptImage.sprite = mapping.Icone;
            InputPromptImage.color = mapping.Color;
        }
        else
        {
            InputPromptImage.gameObject.SetActive(false);
            InputPromptText.gameObject.SetActive(true);

            InputPromptText.text = inputName;
        }
    }


    static public void PushFocusLock()
    {
        s_FocusLock += 1;
    }

    static public void PopFocusLock()
    {
        s_FocusLock = Mathf.Max(0, s_FocusLock - 1);
    }
}
