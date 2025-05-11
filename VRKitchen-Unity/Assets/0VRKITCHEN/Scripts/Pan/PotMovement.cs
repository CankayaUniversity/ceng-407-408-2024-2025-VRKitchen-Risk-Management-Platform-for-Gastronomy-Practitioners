using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class PotGrabFollower : MonoBehaviour
{
    public XRBaseInteractor interactor; // Tutulan el
    private Rigidbody rb;
    private bool isHeld = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnGrab(XRBaseInteractor interactor)
    {
        this.interactor = interactor;
        isHeld = true;
    }

    public void OnRelease(XRBaseInteractor interactor)
    {
        if (this.interactor == interactor)
        {
            this.interactor = null;
            isHeld = false;
        }
    }

    void FixedUpdate()
    {
        if (isHeld && interactor != null)
        {
            rb.MovePosition(interactor.transform.position);
            rb.MoveRotation(interactor.transform.rotation);
        }
    }
}
