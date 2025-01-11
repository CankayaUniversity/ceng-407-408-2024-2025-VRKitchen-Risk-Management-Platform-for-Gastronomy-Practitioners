using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UITooltipReceiver : MonoBehaviour
{
    private static HashSet<UITooltipReceiver> s_Receivers = new HashSet<UITooltipReceiver>();
    
    public virtual void Activate()
    {
        s_Receivers.Add(this);
    }

    public virtual void Deactivate()
    {
        s_Receivers.Remove(this);
    }

    private void OnDestroy()
    {
        Deactivate();
    }
    
    protected abstract void SetTooltip_Internal(UIComponentInfo.ComponentInfoData info);

    public static void SetTooltip(UIComponentInfo.ComponentInfoData info)
    {
        foreach (var receiver in s_Receivers)
        {
            receiver.SetTooltip_Internal(info);
        }
    }
}
