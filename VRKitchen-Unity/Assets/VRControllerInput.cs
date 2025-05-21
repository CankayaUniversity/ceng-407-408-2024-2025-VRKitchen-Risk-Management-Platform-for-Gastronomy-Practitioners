using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRControllerInput : MonoBehaviour
{
    private InputDevice leftController;
    private InputDevice rightController;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    void OnEnable()
    {
        InputDevices.deviceConnected += OnDeviceConnected;
        TryInitializeControllers();
    }

    void OnDisable()
    {
        InputDevices.deviceConnected -= OnDeviceConnected;
    }

    void OnDeviceConnected(InputDevice device)
    {
        if (!leftController.isValid || !rightController.isValid)
        {
            TryInitializeControllers();
        }
    }

    void TryInitializeControllers()
    {
        List<InputDevice> devices = new List<InputDevice>();

        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, devices);
        if (devices.Count > 0)
        {
            leftController = devices[0];
            Debug.Log("Left controller initialized: " + leftController.name);
        }
        else
        {
            Debug.LogWarning("No Device found for left controller");
        }

        devices.Clear();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
        if (devices.Count > 0)
        {
            rightController = devices[0];
            Debug.Log("Right controller initialized: " + rightController.name);
        }
        else
        {
            Debug.LogWarning("No Device found for right controller");
        }
    }

    void Update()
    {
        // Make sure the controller is still valid
        if (!rightController.isValid)
        {
            TryInitializeControllers();
            return;
        }

        // Trigger and grip values
        float indexValue = 0f;
        float threeFingersValue = 0f;
        float thumbValue = 0f;

        // Trigger = Index Finger
        if (rightController.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            indexValue = triggerValue;
        }

        // Grip = Middle/Ring/Pinky (Three Fingers)
        if (rightController.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
        {
            threeFingersValue = gripValue;
        }

        // Thumb (typically primaryButton or secondaryButton — boolean, convert to float)
        if (rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool thumbPressed))
        {
            thumbValue = thumbPressed ? 1f : 0f;
        }

        // Update animator
        if (animator != null)
        {
            animator.SetFloat("Index", indexValue);
            animator.SetFloat("ThreeFingers", threeFingersValue);
            animator.SetFloat("Thumb", thumbValue);
        }
    }
}
