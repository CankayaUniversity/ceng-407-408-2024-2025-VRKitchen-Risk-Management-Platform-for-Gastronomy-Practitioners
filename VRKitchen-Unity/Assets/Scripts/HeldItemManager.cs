using System.Collections.Generic;
using UnityEngine;

public class HeldItemManager : MonoBehaviour
{
    private List<string> heldItemTags = new List<string>();

    // Adds an item's tag to the list
    public void AddItemTag(string tag)
    {
        if (!heldItemTags.Contains(tag)) // Avoid duplicates
        {
            heldItemTags.Add(tag);
            Debug.Log($"Item picked up: {tag}. Current held items: {string.Join(", ", heldItemTags)}");
        }
    }

    // Removes an item's tag from the list
    public void RemoveItemTag(string tag)
    {
        if (heldItemTags.Contains(tag))
        {
            heldItemTags.Remove(tag);
            Debug.Log($"Item dropped: {tag}. Current held items: {string.Join(", ", heldItemTags)}");
        }
    }

    // Get the list of held item tags (optional for other scripts)
    public List<string> GetHeldItemTags()
    {
        return new List<string>(heldItemTags); // Return a copy for safety
    }
}
