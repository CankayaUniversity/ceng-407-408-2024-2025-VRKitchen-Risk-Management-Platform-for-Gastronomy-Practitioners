using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GrabbableItem : BaseInteractiveObject
{
    public Item GrantedItem;

    public string FlagName => name + "_Collected";

    protected override void Awake()
    {
        base.Awake();

        //as the key is only created when grabbed, containing the key is the same as it being true, so no need to check
        //the value as it will only be true (if we were extending the game where you could put back some object, then
        //would need to check)
        if (GameStateManager.Instance.Contains(FlagName))
        {
            gameObject.SetActive(false);
        }
    }

    public override string GetName()
    {
        return GrantedItem.Name;
    }

    public override void Interact(NavMeshAgentController controller)
    {
        //TODO : remove once controller doesn't see disabled object anymore
        if(!gameObject.activeSelf)
            return;
        
        var inv = controller.GetComponentInChildren<Inventory>();
        inv.AddItem(GrantedItem);
        
        GameStateManager.Instance.SetValue(FlagName, true);

        gameObject.SetActive(false);
    }
}
