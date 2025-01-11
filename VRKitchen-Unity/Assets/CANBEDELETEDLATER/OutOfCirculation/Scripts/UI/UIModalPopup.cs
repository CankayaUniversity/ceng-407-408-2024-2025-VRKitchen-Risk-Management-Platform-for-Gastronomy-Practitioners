using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIModalPopup : MonoBehaviour
{
    public TextMeshProUGUI Message;
    public Button ConfirmButton;
    public Button CancelButton;

    public void Show(string message, string confirmText, string cancelText, System.Action onConfirm, Action onCancel)
    {
        UAP_AccessibilityManager.Say("Popup : " + message, false, true, UAP_AudioQueue.EInterrupt.All);
        
        gameObject.SetActive(true);
        
        EventSystem.current.SetSelectedGameObject(CancelButton.gameObject);
        
        Message.text = message;
        
        ConfirmButton.onClick.RemoveAllListeners();
        CancelButton.onClick.RemoveAllListeners();

        //todo : store reference instead of querying everytime, but will happen only on displaying popup so quite rare
        ConfirmButton.GetComponentInChildren<TextMeshProUGUI>().text = confirmText;
        ConfirmButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            onConfirm.Invoke();
        });
        
        CancelButton.GetComponentInChildren<TextMeshProUGUI>().text = cancelText;
        CancelButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            onCancel.Invoke();
        });
    }

    public void ChangeMessage(string newMessage)
    {
        Message.text = newMessage;
    }
}
