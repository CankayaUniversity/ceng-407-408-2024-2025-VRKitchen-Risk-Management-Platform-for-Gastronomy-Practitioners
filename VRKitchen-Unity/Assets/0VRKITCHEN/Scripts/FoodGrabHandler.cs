using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class FoodGrabHandler : MonoBehaviour
{
    private Collider[] colliders;
    private Rigidbody rb;

    void Awake()
    {
        colliders = GetComponentsInChildren<Collider>(true);
        rb = GetComponent<Rigidbody>();

        var grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        foreach (var col in colliders)
            col.enabled = true; // Collider'lar� a��k tut

        if (rb != null)
            rb.isKinematic = false; // Grab yap�l�nca fizik tekrar aktif
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        // TriggerExit tetiklenirse PanFoodHolder taraf�ndan kontrol edilir
    }
}
