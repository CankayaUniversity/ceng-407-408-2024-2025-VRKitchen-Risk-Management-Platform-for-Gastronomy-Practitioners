using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class StateInfoHandler : MonoBehaviour, UIComponentInfo.IComponentStateDataProvider, ISubmitHandler, IPointerClickHandler
{
    public string GetState()
    {
        return GetState_Internal();
    }

    protected abstract string GetState_Internal();

    public void OnSubmit(BaseEventData eventData)
    {
        UAP_AccessibilityManager.Say(GetState(), true, true, UAP_AudioQueue.EInterrupt.All);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UAP_AccessibilityManager.Say(GetState(), true, true, UAP_AudioQueue.EInterrupt.All);
    }
}
