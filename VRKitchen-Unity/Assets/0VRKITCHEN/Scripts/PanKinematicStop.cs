using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PanKinematicStop : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.thisCollider.name == "PanBottom" && collision.gameObject.CompareTag("Food"))
            {
                Rigidbody rb = collision.rigidbody;
                if (rb != null)
                {
                    collision.transform.SetParent(transform);
                    rb.isKinematic = true;

                }
            }
        }
    }

    //private void OnCollisionExit(Collision collision)
    //{
    //    foreach (ContactPoint contact in collision.contacts)
    //    {
    //        if (contact.thisCollider.name == "PanBottom" && collision.gameObject.CompareTag("Food"))
    //        {
    //            Rigidbody rb = collision.rigidbody;
    //            XRGrabInteractable grab = collision.gameObject.GetComponent<XRGrabInteractable>();

    //            if (rb != null && grab != null && grab.isSelected) // Eðer elde tutuluyorsa
    //            {
    //                rb.isKinematic = false;
    //                collision.transform.SetParent(null); // Artýk tencerenin çocuðu deðil
                    
    //            }
    //        }
    //    }
    //}
}