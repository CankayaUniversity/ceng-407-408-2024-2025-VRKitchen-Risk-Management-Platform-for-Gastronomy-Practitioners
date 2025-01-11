using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIHelpers
{
    public static void GetInputTextOrIcon(InputAction action, string bindingGroup, out ControlIconSelector.Mapping iconMapping, out string name)
    {
        int idx = action.GetBindingIndex(bindingGroup);
        var displayString = action.GetBindingDisplayString(idx, out string device, out string controlPath);
        iconMapping =
            ControlManager.ControlIconSelector.GetMappingForSetAndPath(SaveSystem.CurrentSettings.IconeSet,
                controlPath);

        name = iconMapping == null ? displayString : null;
    }
}
