using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;

/// <summary>
/// Handle everything related to control and InputActionAsset etc.
/// For now assume a single user all along the session as our game only require that, may be extended to create and
/// handle multiple users.
/// </summary>
public class ControlManager
{
    public enum ControlType
    {
        Mouse,
        Keyboard,
        Gamepad
    }

    private static ControlManager s_Instance;

    public static InputActionAsset CurrentInput => s_Instance.m_CurrentInput;
    public static InputControlScheme CurrentControlScheme => s_Instance.m_CurrentControlScheme;
    public static ControlIconSelector ControlIconSelector => s_Instance.m_ControlIconSelector;
    public static ControlType CurrentControlType => s_Instance.m_CurrentControlType;

    public static InputAction UISubmitAction => s_Instance.m_UISubmitAction;
    public static InputAction UICancelAction => s_Instance.m_UICancelAction;

    public static event Action OnControlTypeChanged;

    private InputActionAsset m_CurrentInput;
    private InputControlScheme m_CurrentControlScheme;
    private ControlIconSelector m_ControlIconSelector;
    private ControlType m_CurrentControlType;
    
    private InputUser m_User;

    private InputActionMap m_GameplayActionMap;
    private InputActionMap m_UIActionMap;
    private InputAction m_UISubmitAction;
    private InputAction m_UICancelAction;

    static ControlManager()
    {
        s_Instance = new ControlManager();
    }

    public static void Init()
    {
        s_Instance.InternalInit();
    }

    /// <summary>
    /// This will disable all gameplay action and enable all UI action. Called by diverse UI code when popup/menu open
    /// </summary>
    public static void SwitchToUI()
    {
        s_Instance.m_UIActionMap.Enable();
        s_Instance.m_GameplayActionMap.Disable();
    }

    /// <summary>
    /// This will enable all Gameplay action and disable UI actions, useful when going back to game closing a UI popup/menu
    /// </summary>
    public static void SwitchToGameplay()
    {
        s_Instance.m_UIActionMap.Disable();
        s_Instance.m_GameplayActionMap.Enable();
    }
    

    void InternalInit()
    {
        m_CurrentInput = Object.Instantiate(DataReference.Instance.DefaultInputActionAsset);
        
        m_ControlIconSelector = Resources.Load<ControlIconSelector>("ControlIconSelector");
        m_ControlIconSelector.Init();
        
        //generate a single user (could be extend to move that into a CreateUser function that any character could 
        //call to get a reference to their own suer and keep the user in a list here
        m_User = InputUser.CreateUserWithoutPairedDevices();
        m_User.AssociateActionsWithUser(m_CurrentInput);
        
        InputUser.onUnpairedDeviceUsed += UnpairedDeviceActivityDetected;
        ++InputUser.listenForUnpairedDeviceActivity;

        m_GameplayActionMap = m_CurrentInput.FindActionMap("Gameplay");
        m_UIActionMap = m_CurrentInput.FindActionMap("UI");

        m_UISubmitAction = m_UIActionMap.FindAction("Interact");
        m_UICancelAction = m_UIActionMap.FindAction("Cancel");
    }
    
    //TODO : Never called and shouldn't be needed as this class live for the whole execution, but keeping it here until
    //release when we can remove if really not needed
    private void OnDestroy()
    {
        InputUser.onUnpairedDeviceUsed -= UnpairedDeviceActivityDetected;
        --InputUser.listenForUnpairedDeviceActivity;
    }
    
    void UnpairedDeviceActivityDetected(InputControl control, InputEventPtr ptr)
    {
        InputControlScheme scheme;
        
        if (InputControlScheme.FindControlSchemeForDevices(InputUser.GetUnpairedInputDevices(), 
            m_CurrentInput.controlSchemes,
            out scheme,
            out var result, mustIncludeDevice: control.device))
        {
            m_CurrentControlScheme = scheme;
            m_User.UnpairDevices();

            //found a scheme for that device
            var unpairedDevice = result.devices;

            switch (m_CurrentControlScheme.name)
            {
                case "Mouse" :
                    m_CurrentControlType = ControlType.Mouse;
                    OnControlTypeChanged?.Invoke();
                    break;
                case "Keyboard" :
                    m_CurrentControlType = ControlType.Keyboard;
                    OnControlTypeChanged?.Invoke();
                    break;
                case "Gamepad" :
                    m_CurrentControlType = ControlType.Gamepad;
                    OnControlTypeChanged?.Invoke();
                    break;
            }

            foreach (var device in unpairedDevice)
            {
                m_User = InputUser.PerformPairingWithDevice(device, m_User);
            }

            result.Dispose();
        }
    }
}
