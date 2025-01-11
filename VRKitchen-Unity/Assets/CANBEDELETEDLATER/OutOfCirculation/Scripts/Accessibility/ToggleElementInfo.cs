using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleElementInfo : StateInfoHandler
{
    private Toggle m_Toggle;

    private void Awake()
    {
        m_Toggle = GetComponent<Toggle>();
    }

    protected override string GetState_Internal()
    {
        return m_Toggle.isOn ? "On" : "Off";
    }
}
