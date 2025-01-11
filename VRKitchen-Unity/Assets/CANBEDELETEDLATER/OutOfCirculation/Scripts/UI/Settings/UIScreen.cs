using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Define one screen (page) of the setting menu. This is subclassed by all pages (game, video, sound etc.)
/// </summary>
public abstract class UIScreen : MonoBehaviour
{
    protected string m_SavedSettings;
    private bool m_Changed;

    private bool m_WasKeyInput = false;

    private ScrollRect m_ScrollRect;
    private GameObject m_CurrentlySelected;

    //Called by the setting menu when it is built for the first time
    public virtual void Build()
    {
        
    }
    
    //call when the screen is displayed
    public virtual void Display()
    {
        m_SavedSettings = JsonUtility.ToJson(SaveSystem.CurrentSettings);
        
        gameObject.SetActive(true);

        m_ScrollRect.verticalNormalizedPosition = 1.0f;
        
        UISettingMenu.Instance.BottomBar.SetupBar(() =>
        {
            SaveChange(() => { });
        }, UndoChange);
        
        DetectInputChange_Internal(false);

        ControlManager.OnControlTypeChanged += DetectInputChange;

        SetupNavigation();
    }

    void SetupNavigation()
    {
        //setup the last control on the screen to select the close button on the bar and be selected by up from any button
        var lastControl = GetLastControl();

        if(lastControl == null)
            return;
        
        var closeButton = UISettingMenu.Instance.BottomBar.CloseButton;
        var saveButton = UISettingMenu.Instance.BottomBar.SaveButton;
        var cancelButton = UISettingMenu.Instance.BottomBar.UndoButton;
        
        var nav = lastControl.navigation;
        nav.selectOnDown = closeButton;
        lastControl.navigation = nav;

        nav = closeButton.navigation;
        nav.selectOnUp = lastControl;
        closeButton.navigation = nav;
        
        nav = saveButton.navigation;
        nav.selectOnUp = lastControl;
        saveButton.navigation = nav;
        
        nav = cancelButton.navigation;
        nav.selectOnUp = lastControl;
        cancelButton.navigation = nav;
    }

    protected abstract void SetupKeyInput();

    void DetectInputChange()
    {
        DetectInputChange_Internal(true);
    }
    
    void DetectInputChange_Internal(bool setupKeyInput)
    {
        if (ControlManager.CurrentControlType != ControlManager.ControlType.Mouse)
        {
            if (!m_WasKeyInput)
            {
                m_WasKeyInput = true;
                if(setupKeyInput) SetupKeyInput();
                
                m_CurrentlySelected = null;
            }
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            m_WasKeyInput = false;
        }
    }

    public void Close(System.Action closeAction)
    {
        if (m_Changed)
        {
            UISettingMenu.Instance.ModalPopup.Show("You have unsaved changes. <b>Save</b> to apply them or select <b>undo</b>.",
                "Save",
                "Undo",
                () =>
                {
                    SaveChange(() =>
                    {
                        InternalClose(closeAction);
                    });
                },
                () =>
                {
                    UndoChange();
                    InternalClose(closeAction);
                });
        }
        else
        {
           InternalClose(closeAction);
        }
    }

    protected virtual void InternalClose(System.Action closeAction)
    {
        ControlManager.OnControlTypeChanged -= DetectInputChange;
        gameObject.SetActive(false);
        closeAction.Invoke();

        m_WasKeyInput = false;
    }

    protected void Dirty()
    {
        m_Changed = true;
        UISettingMenu.Instance.BottomBar.NotifyInteractable(true);
    }

    protected void Save()
    {
        m_Changed = false;
        SaveSystem.SaveSetting();
        //we take a new snapshot of the setting as they are the new "default"
        m_SavedSettings = JsonUtility.ToJson(SaveSystem.CurrentSettings);
        
        UISettingMenu.Instance.BottomBar.NotifyInteractable(false);
    }
    
    protected virtual void SaveChange(System.Action onSave)
    {
        Save();
        onSave.Invoke();
    }

    protected virtual void UndoChange()
    {
        JsonUtility.FromJsonOverwrite(m_SavedSettings, SaveSystem.CurrentSettings);

        m_Changed = false;
        UISettingMenu.Instance.BottomBar.NotifyInteractable(false);
    }


    protected virtual void Awake()
    {
        m_ScrollRect = GetComponentInChildren<ScrollRect>();
    }

    protected virtual void Update()
    {
        //we check if the selection changed, and try to frame the selection in the scroll rect if it have
        var selectedGameObject = EventSystem.current.currentSelectedGameObject;
        
        if (selectedGameObject != m_CurrentlySelected)
        {
            m_CurrentlySelected = selectedGameObject;
            FrameSelected();
        }
    }

    public abstract Selectable GetLastControl();
    public abstract Selectable GetFirstControl();

    void FrameSelected()
    {
        if(m_CurrentlySelected == null)
            return;

        //Note : all of that computation will only happen once when a new selection is detected. To handle that 
        //more cleanly, we could subclass ScrollRect to do this automatically in there, but the code for this
        //project is complex enough as it is, so we keep it simple and here. This is not the most efficient but
        //should be good enough for most use case (always profile, never assume). If profiling show that part 
        //being culprit in bad performance in bigger project, can always refactor that into a custom scroll rect. 
        
        var viewportHeight = m_ScrollRect.viewport.rect.height;
        var contentHeight = m_ScrollRect.content.rect.height;
        
        //this is the y of the bottom right when scrolled all the way up
        var maxY = contentHeight - viewportHeight;
        
        var rectTransform = m_CurrentlySelected.GetComponent<RectTransform>();
        Vector3[] worldCorner = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorner);

        var bottomLeftLocal = m_ScrollRect.content.InverseTransformPoint(worldCorner[0]);
        var topLeftLocal = m_ScrollRect.content.InverseTransformPoint(worldCorner[1]);
        
        //scroll rect vertical position is 0 when scrolled all the way to the bottom and 1 all the way to the top
        //so we work in that frame (y = 0 is bottom left when scrolled all the way down, y = content height is the
        //top left when scroll all the way to the top)
        //But our scroll rect content are organized top to bottom, with the anchor at the top. So content position
        //is a negative y from the top. So we change that back to y = 0 at the bottom and y = height at the top
        bottomLeftLocal.y += contentHeight;
        topLeftLocal.y += contentHeight;

        var elementHeight = topLeftLocal.y - bottomLeftLocal.y;

        //this give us, in the content space, which is the y seen by the bottom left of the view and the y at the
        //top left of the view
        var bottomViewY = maxY * m_ScrollRect.verticalNormalizedPosition;
        var topViewY = bottomViewY + viewportHeight;

        //then we check against the rect of our element to scroll enough if it is not in view
        if (bottomLeftLocal.y < bottomViewY)
        {
            //we scroll enough to bring back the bottom (+ half it's height for clarity) in view
            m_ScrollRect.verticalNormalizedPosition -= (bottomViewY - bottomLeftLocal.y + elementHeight * 0.5f) / maxY;
        }
        else if (topLeftLocal.y > topViewY)
        {
            m_ScrollRect.verticalNormalizedPosition += (topLeftLocal.y - topViewY + elementHeight * 0.5f) / maxY;
        }
    }
}
