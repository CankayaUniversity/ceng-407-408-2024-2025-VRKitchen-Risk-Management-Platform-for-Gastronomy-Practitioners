using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookableItem : BaseInteractiveObject
{
    public Item ItemInfo;

    public override string GetName()
    {
        return ItemInfo.Name;
    }

    public override void Interact(NavMeshAgentController controller)
    {
        base.Interact(controller);
        
        ControlManager.SwitchToUI();
        UIItemInfoPopup.Instance.Show(ItemInfo, () =>
        {
            ControlManager.SwitchToGameplay();
        } );
    }
}
