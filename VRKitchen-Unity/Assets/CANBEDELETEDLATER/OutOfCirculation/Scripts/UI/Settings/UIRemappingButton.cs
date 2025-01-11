using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIRemappingButton : Button
{
    public Action OnSelected;
    
    
    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);

        OnSelected?.Invoke();
    }
}
