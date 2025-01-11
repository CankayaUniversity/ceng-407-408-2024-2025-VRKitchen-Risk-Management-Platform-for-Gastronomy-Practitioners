using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIComponentInfo : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    [System.Serializable]
    public class ComponentInfoData
    {
        public string Name;
        public string UsageHint;
        public string Tooltip;
    }
    
    public interface IComponentInfoDataProvider
    {
        ComponentInfoData GetComponentInfo();
    }

    public interface IComponentStateDataProvider
    {
        string GetState();
    }

    public ComponentInfoData InfoData;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ControlManager.CurrentControlType == ControlManager.ControlType.Mouse)
        {
            ProcessComponent();
        }
    }
    
    public void OnSelect(BaseEventData eventData)
    {
        if (ControlManager.CurrentControlType != ControlManager.ControlType.Mouse)
        {
            ProcessComponent();
        }
    }

    public void ProcessComponent()
    {
        var data = GetComponent<IComponentInfoDataProvider>()?.GetComponentInfo();
        if (data == null) data = InfoData;

        var stateInfo = GetComponent<IComponentStateDataProvider>()?.GetState();
        
        UITooltipReceiver.SetTooltip(data);
        string textToSay = data.Name + ". ";
        if (stateInfo != null) textToSay += stateInfo + ". ";
        if (!string.IsNullOrEmpty(data.UsageHint)) textToSay += data.UsageHint;
        if (!string.IsNullOrEmpty(data.Tooltip)) textToSay += data.Tooltip;

        UAP_AccessibilityManager.Say(textToSay, UAP_AudioQueue.EAudioType.App, true, true, UAP_AudioQueue.EInterrupt.All);
    }
}
