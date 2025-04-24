using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class FoodPickupKinematic : XRGrabInteractable
{
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            transform.SetParent(null); 
            rb.isKinematic = false;    
            
        }
    }
}
