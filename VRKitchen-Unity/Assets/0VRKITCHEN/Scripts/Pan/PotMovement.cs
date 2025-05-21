using UnityEngine;

public class PanFollowHand : MonoBehaviour
{
    public Transform followTarget; // Referans nokta (el üzerindeki bir boþ GameObject)
    public float followSpeed = 15f;
    public float rotateSpeed = 10f;

    private Rigidbody rb;
    private bool isHeld = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (isHeld)
        {
            Vector3 posDelta = followTarget.position - rb.position;
            rb.velocity = posDelta * followSpeed;

            Quaternion rotDelta = followTarget.rotation * Quaternion.Inverse(rb.rotation);
            rotDelta.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180) angle -= 360;
            rb.angularVelocity = axis * angle * Mathf.Deg2Rad * rotateSpeed;
        }
        else
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void GrabPan()
    {
        isHeld = true;
    }

    public void ReleasePan()
    {
        isHeld = false;
    }
}
