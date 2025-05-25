using UnityEngine;

public class UIFollower : MonoBehaviour
{
    public Transform cameraTransform; 
    public Transform uiCanvas;        

    void Start()
    {
        PositionInFrontOfCamera();
    }

    void PositionInFrontOfCamera()
    {
        if (cameraTransform == null || uiCanvas == null) return;

        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;

        uiCanvas.position = cameraTransform.position + forward.normalized * 2f + Vector3.up * -0.2f;
        uiCanvas.LookAt(cameraTransform);
        uiCanvas.Rotate(0, 180f, 0); // Flip to face the user
    }
}
