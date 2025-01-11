using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInventorySidebar : UISidebar
{
    public static UIInventorySidebar Instance { get; private set; }
    
    public UIInventoryEntry[] Entries;

    public void Awake()
    {
        Instance = this;
        
        for(int i = 0; i < Entries.Length; ++i)
        {
            var entry = Entries[i];
            
            entry.SetupSidebar(this);
            
            entry.RemoveItem();

            var nav = entry.navigation;
            nav.mode = Navigation.Mode.Explicit;

            if (i > 0)
                nav.selectOnUp = Entries[i - 1];

            if (i < Entries.Length - 1)
                nav.selectOnDown = Entries[i + 1];

            entry.navigation = nav;
        }
        
        InitSystem.RegisterOnInitEvent(() =>
        {
            RegisterFocus(ControlManager.CurrentInput.FindAction("Common/AccessRightSidebar"), Entries[0].gameObject);
        });
    }
    
    public void SyncToInventory(Inventory inv)
    {
        for (int i = 0; i < inv.Content.Count; ++i)
        {
            if (i >= Entries.Length)
            {
                //good enough for our vertical slice, full game could have a scrollable sidebar
                Debug.LogError("Couldn't fit all the inventory on the sidebar");
                return;
            }
            
            Entries[i].DisplayItem(inv.Content[i]);
        }

        //we make sure to disable any additional inventory entry
        for (int i = inv.Content.Count; i < Entries.Length; i++)
        {
            Entries[i].RemoveItem();
        }
    }
}
