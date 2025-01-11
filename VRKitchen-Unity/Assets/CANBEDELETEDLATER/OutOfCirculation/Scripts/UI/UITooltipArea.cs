using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITooltipArea : UITooltipReceiver, UIRoot.IUIInitHandler
{
    public static UITooltipArea Instance { get; private set; }

    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI TooltipText;
    
    public void Init()
    {
        Instance = this;
    }

    protected override void SetTooltip_Internal(UIComponentInfo.ComponentInfoData info)
    {
        if (info != null && !string.IsNullOrEmpty(info.Tooltip))
        {
            TitleText.text = "<u>"+info.Name+"</u>";
            TooltipText.text = info.Tooltip;
        }
        else
        {
            TitleText.text = "";
            TooltipText.text = "";
        }
    }
}
