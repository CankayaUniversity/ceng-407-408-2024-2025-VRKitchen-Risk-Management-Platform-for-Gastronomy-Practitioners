using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PanKinematicStop : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            other.transform.SetParent(transform.parent);

            other.attachedRigidbody.isKinematic = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            Rigidbody rb = other.attachedRigidbody;
            if (other.TryGetComponent<XRGrabInteractable>(out var grab) && grab.isSelected)
            {
                
                other.transform.SetParent(null);
                
                // Fýrlama engelle
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                
                rb.isKinematic = false;

               
            }
        }
    }

}