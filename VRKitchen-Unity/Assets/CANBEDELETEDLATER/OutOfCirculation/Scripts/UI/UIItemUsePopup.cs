using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIItemUsePopup : MonoBehaviour, UIRoot.IUIInitHandler
{
    private static UIItemUsePopup s_Instance;

    public TextMeshProUGUI Text;
    public Button CloseButton;

    protected System.Action m_OnClosed;
    
    public void Init()
    {
        s_Instance = this;
        
        CloseButton.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    public static void DisplayMessage(string message, System.Action onCloseAction = null)
    {
        s_Instance.Text.text = message;
        s_Instance.Show();

        s_Instance.m_OnClosed = onCloseAction;
    }

    public void Show()
    {
        EventSystem.current.SetSelectedGameObject(CloseButton.gameObject);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        m_OnClosed?.Invoke();
        gameObject.SetActive(false);
    }
}
