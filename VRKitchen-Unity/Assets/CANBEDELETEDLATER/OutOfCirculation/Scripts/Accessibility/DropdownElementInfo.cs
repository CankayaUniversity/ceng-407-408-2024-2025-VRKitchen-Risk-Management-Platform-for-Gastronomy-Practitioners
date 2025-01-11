using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropdownElementInfo : StateInfoHandler
{
    private TMP_Dropdown m_Dropdown;

    private void Awake()
    {
        m_Dropdown = GetComponent<TMP_Dropdown>();
    }

    protected override string GetState_Internal()
    {
        return m_Dropdown?.options[m_Dropdown.value].text;
    }
}
