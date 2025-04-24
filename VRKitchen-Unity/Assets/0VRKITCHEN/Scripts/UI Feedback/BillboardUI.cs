using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    public Camera cameraToLookAt;

    void Update()
    {
        if (cameraToLookAt != null)
        {
            // This makes it turn to face you left/right only
            Vector3 lookDir = cameraToLookAt.transform.position - transform.position;
            lookDir.y = 0; // Lock vertical tilt
            transform.forward = lookDir.normalized;
        }
    }
}
