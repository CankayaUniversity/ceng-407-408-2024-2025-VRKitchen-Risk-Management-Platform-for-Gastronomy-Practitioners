using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRControllerInput : MonoBehaviour
{
    public XRNode handType = XRNode.RightHand; // Set in Inspector (LeftHand or RightHand)
    private InputDevice controller;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        TryInitializeController();
    }

    void OnEnable()
    {
        InputDevices.deviceConnected += OnDeviceConnected;
        TryInitializeController();
    }

    void OnDisable()
    {
        InputDevices.deviceConnected -= OnDeviceConnected;
    }

    void OnDeviceConnected(InputDevice device)
    {
        if (!controller.isValid)
        {
            TryInitializeController();
        }
    }

    void TryInitializeController()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(handType, devices);
        if (devices.Count > 0)
        {
            controller = devices[0];
            Debug.Log($"{handType} controller initialized: {controller.name}");
        }
        else
        {
            //Debug.LogWarning($"No Device found for {handType}");
        }
    }

    void Update()
    {
        if (!controller.isValid)
        {
            TryInitializeController();
            return;
        }

        float indexValue = 0f;
        float threeFingersValue = 0f;
        float thumbValue = 0f;

        controller.TryGetFeatureValue(CommonUsages.trigger, out indexValue);
        controller.TryGetFeatureValue(CommonUsages.grip, out threeFingersValue);

        // Try to use primaryTouch for thumb, fallback to primaryButton
        if (controller.TryGetFeatureValue(CommonUsages.primaryTouch, out bool thumbTouch))
            thumbValue = thumbTouch ? 1f : 0f;
        else if (controller.TryGetFeatureValue(CommonUsages.primaryButton, out bool thumbPressed))
            thumbValue = thumbPressed ? 1f : 0f;

        if (animator != null)
        {
            animator.SetFloat("Index", indexValue);
            animator.SetFloat("ThreeFingers", threeFingersValue);
            animator.SetFloat("Thumb", thumbValue);
        }
    }
}
