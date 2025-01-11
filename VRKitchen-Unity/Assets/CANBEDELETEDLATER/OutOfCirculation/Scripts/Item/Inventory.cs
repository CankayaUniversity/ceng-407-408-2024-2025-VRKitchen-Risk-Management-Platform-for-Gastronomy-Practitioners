using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<Item> Content;


    public void AddItem(Item item)
    {
        Content.Add(item);
        
        UIInventorySidebar.Instance.SyncToInventory(this);
    }
}
