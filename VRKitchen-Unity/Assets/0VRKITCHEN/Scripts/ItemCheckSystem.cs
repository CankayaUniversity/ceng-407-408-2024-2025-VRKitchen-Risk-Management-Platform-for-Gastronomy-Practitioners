using System.Collections.Generic;
using UnityEngine;

public class ItemCheckSystem : MonoBehaviour
{
    [SerializeField] private HandItemTracker leftHandTracker;
    [SerializeField] private HandItemTracker rightHandTracker;
    [SerializeField] private ItemTracker itemTracker;

    public bool CheckIfTagExists(string requiredTag)
    {
        
        List<string> handTags = new List<string>();
        handTags.AddRange(leftHandTracker.GetHeldItemTags());
        handTags.AddRange(rightHandTracker.GetHeldItemTags());

        
        List<string> itemTags = itemTracker.GetTouchingItemTags();

        
        if (handTags.Contains(requiredTag)) //  || itemTags.Contains(requiredTag) ekle ayrý olarak
        {
            
            return true;
        }

       
        return false;
    }
    private void Update()//test
    {
        bool hasKnifeOrCucumber = CheckIfTagExists("Knife") || CheckIfTagExists("Cucumber");
        Debug.Log(hasKnifeOrCucumber);
    }
}
