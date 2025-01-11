using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class StartPopup : MonoBehaviour
{
    public Toggle ToggleAudioNarration;
    public UIStartScreen StartScreen;
    
    public void Start()
    {
        ToggleAudioNarration.SetIsOnWithoutNotify(UAP_AccessibilityManager.IsEnabled());
        ToggleAudioNarration.onValueChanged.AddListener(on =>
        {
            AccessibilityHelper.ToggleTTS(on);
        });


        EventSystem.current.SetSelectedGameObject(ToggleAudioNarration.gameObject);
        var elem = ToggleAudioNarration.GetComponent<UIComponentInfo>();
        elem.ProcessComponent();

        ControlManager.OnControlTypeChanged += HandleControlChange;
    }

    void HandleControlChange()
    {
        if (ControlManager.CurrentControlType != ControlManager.ControlType.Mouse)
        {
            EventSystem.current.SetSelectedGameObject(ToggleAudioNarration.gameObject);
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
        StartScreen.BackToMenu();
        
        ControlManager.OnControlTypeChanged -= HandleControlChange;
    }
}
