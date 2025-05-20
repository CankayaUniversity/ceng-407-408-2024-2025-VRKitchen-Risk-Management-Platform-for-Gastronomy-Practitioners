using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HandTracker : MonoBehaviour
{
    [Header("XR Hands")]
    public XRDirectInteractor leftHand;
    public XRDirectInteractor rightHand;

    private GameObject lastLeftHeld;
    private GameObject lastRightHeld;

    private void Update()
    {
        CheckHandChange(leftHand, ref lastLeftHeld, "left hand");
        CheckHandChange(rightHand, ref lastRightHeld, "right hand");
    }

    private void CheckHandChange(XRDirectInteractor hand, ref GameObject lastHeld, string handName)
    {
        GameObject currentHeld = hand.selectTarget != null ? hand.selectTarget.gameObject : null;

        if (currentHeld != lastHeld)
        {
            if (currentHeld != null)
            {
                string itemName = GetItemName(currentHeld);
                SendHoldMessage(itemName, handName);
            }
            else
            {
                SendReleaseMessage(handName);
            }

            lastHeld = currentHeld;
        }
    }

    private string GetItemName(GameObject obj)
    {
        FoodInstance food = obj.GetComponent<FoodInstance>();
        if (food != null && food.foodData != null)
        {
            return food.foodData.foodName.ToLower();
        }

        return obj.name.ToLower();
    }

    private void SendHoldMessage(string itemName, string handName)
    {
        GameActionManager manager = FindObjectOfType<GameActionManager>();
        if (manager != null)
        {
            manager.RegisterAction($"I'm holding the {itemName} in my {handName}. What’s the next step?");
        }
    }

    private void SendReleaseMessage(string handName)
    {
        GameActionManager manager = FindObjectOfType<GameActionManager>();
        if (manager != null)
        {
            manager.RegisterAction($"I released the item from my {handName}.");
        }
    }
}