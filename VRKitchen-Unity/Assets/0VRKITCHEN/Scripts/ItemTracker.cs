using System.Collections.Generic;
using UnityEngine;

public class ItemTracker : MonoBehaviour
{
    public List<string> touchingItemTags = new List<string>();
    //[SerializeField] private QuestSystem questSystem;
    private void OnCollisionEnter(Collision collision)
    {
        string itemTag = collision.gameObject.tag;
        if (!touchingItemTags.Contains(itemTag))
        {
            touchingItemTags.Add(itemTag);
            
           // questSystem.OnItemPlaced(itemTag);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        string itemTag = collision.gameObject.tag;
        if (touchingItemTags.Contains(itemTag))
        {
            touchingItemTags.Remove(itemTag);
            
        }
    }

    public List<string> GetTouchingItemTags()
    {
        return new List<string>(touchingItemTags);
    }
}
