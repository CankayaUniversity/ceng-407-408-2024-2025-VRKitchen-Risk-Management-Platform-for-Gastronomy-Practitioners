using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRControllerInput : MonoBehaviour
{
    private InputDevice leftController;
    private InputDevice rightController;

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
        // Check if devices are still valid (can disconnect/reconnect)
        if (!leftController.isValid || !rightController.isValid)
        {
            TryInitializeControllers();
        }

        // Trigger values
        if (rightController.TryGetFeatureValue(CommonUsages.trigger, out float rightTriggerValue))
        {
            Debug.Log($"Right Trigger: {rightTriggerValue}");
        }

        if (leftController.TryGetFeatureValue(CommonUsages.trigger, out float leftTriggerValue))
        {
            Debug.Log($"Left Trigger: {leftTriggerValue}");
        }

        // Grip (grab) values
        if (rightController.TryGetFeatureValue(CommonUsages.grip, out float rightGripValue))
        {
            Debug.Log($"Right Grip: {rightGripValue}");
        }

        if (leftController.TryGetFeatureValue(CommonUsages.grip, out float leftGripValue))
        {
            Debug.Log($"Left Grip: {leftGripValue}");
        }
    }
}
