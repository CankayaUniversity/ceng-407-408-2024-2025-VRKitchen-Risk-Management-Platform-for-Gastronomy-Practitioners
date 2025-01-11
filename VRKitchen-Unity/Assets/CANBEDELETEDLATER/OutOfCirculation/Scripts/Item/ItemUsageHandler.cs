using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// When an Item is used from inventory, it will try to call Used on that singleton. If there is none in the scene,
/// a popup notifying the player that the item can't be used here will appear. Similarly if there is one in the scene
/// but there is no mapping for the Used object (so no linked action for using that item in this scene) it will display
/// the same popup.
///
/// Otherwise, it will call the actions linked to that object in that scene.
/// </summary>
public class ItemUsageHandler : MonoBehaviour
{
    public static ItemUsageHandler Instance { get; protected set; }

    [System.Serializable]
    public struct UseMapping
    {
        public Item Item;
        public UnityEvent OnUsed;
    }

    public UseMapping[] Mappings;

    private Dictionary<Item, UnityEvent> m_ItemToEventLookup = new Dictionary<Item, UnityEvent>();

    private void Awake()
    {
        Instance = this;

        foreach (var mapping in Mappings)
        {
            m_ItemToEventLookup.Add(mapping.Item, mapping.OnUsed);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static void Used(Item item, System.Action postUsedAction = null)
    {
        AudioManager.Instance.PlayPointSFX(item.UseSFX, Camera.main.transform.position, false);
        
        if (Instance == null)
        {
            //no handler, object can't be used in that scene.
            UIItemUsePopup.DisplayMessage("Sorry, you can’t use that item here.", postUsedAction);
            return;
        }

        if (Instance.m_ItemToEventLookup.TryGetValue(item, out var evt))
        {
            evt.Invoke();
        }
        else
        {
            //no use event
            UIItemUsePopup.DisplayMessage("Sorry, you can’t use that item here.", postUsedAction);
        }
    }
}
