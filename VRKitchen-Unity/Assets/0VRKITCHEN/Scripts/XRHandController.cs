using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public enum HandType
{
    Left,
    Right
}

public class XRHandController : MonoBehaviour
{
    public HandType handType;
    public float thumbMoveSpeed = 0.1f;

    private Animator animator;
    private InputDevice inputDevice;

    public UnityEngine.InputSystem.InputActionProperty handTriggerValue;
    public UnityEngine.InputSystem.InputActionProperty handGrapValue;

    private float indexValue;
    private float thumbValue;
    private float threeFingersValue;



    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        inputDevice = GetInputDevice();
        print("Input Device: " + inputDevice.name);
    
        List<InputDevice> allDevices = new List<InputDevice>();
        InputDevices.GetDevices(allDevices);

        foreach (var device in allDevices)
        {
            Debug.Log($"Device: {device.name}, Characteristics: {device.characteristics}");

            foreach (InputDeviceCharacteristics flag in System.Enum.GetValues(typeof(InputDeviceCharacteristics)))
            {
                if ((device.characteristics & flag) != 0)
                {
                    Debug.Log($" - {flag}");
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        AnimateHand();

        //print("trigger value: " + handTriggerValue.action.ReadValue)
    }

    InputDevice GetInputDevice()
    {
        InputDeviceCharacteristics controllerCharacteristic = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller;

        if (handType == HandType.Left)
        {
            controllerCharacteristic = controllerCharacteristic | InputDeviceCharacteristics.Left;
        }
        else
        {
            controllerCharacteristic = controllerCharacteristic | InputDeviceCharacteristics.Right;
        }

        List<InputDevice> inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristic, inputDevices);

        return inputDevices[0];
    }

    void AnimateHand()
    {
        inputDevice.TryGetFeatureValue(CommonUsages.trigger, out indexValue);
        inputDevice.TryGetFeatureValue(CommonUsages.grip, out threeFingersValue);

        inputDevice.TryGetFeatureValue(CommonUsages.primaryTouch, out bool primaryTouched);
        inputDevice.TryGetFeatureValue(CommonUsages.secondaryTouch, out bool secondaryTouched);

        if (primaryTouched || secondaryTouched)
        {
            thumbValue += thumbMoveSpeed;
        }
        else
        {
            thumbValue -= thumbMoveSpeed;
        }

        thumbValue = Mathf.Clamp(thumbValue, 0, 1);

        animator.SetFloat("Index", indexValue);
        animator.SetFloat("ThreeFingers", threeFingersValue);
        animator.SetFloat("Thumb", thumbValue);
    }
}