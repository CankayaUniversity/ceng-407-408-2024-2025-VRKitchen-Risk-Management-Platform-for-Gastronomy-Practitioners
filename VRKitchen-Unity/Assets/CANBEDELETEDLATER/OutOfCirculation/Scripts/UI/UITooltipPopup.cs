using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITooltipPopup : UITooltipReceiver
{
    public TextMeshProUGUI TooltipText;

    public override void Activate()
    {
        base.Activate();
        
        gameObject.SetActive(true);
    }

    public override void Deactivate()
    {
        base.Deactivate();
        
        gameObject.SetActive(false);
    }

    protected override void SetTooltip_Internal(UIComponentInfo.ComponentInfoData info)
    {
        TooltipText.text = info.Tooltip;
    }
}
