using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIKeyboardControlSettingScreen : UIControlSettingBase
{
    public override void Build()
    {
        base.Build();
        
        var input = ControlManager.CurrentInput;
        
        //This is the keyboard option screen so we only get the Mouse/Keyboard Scheme
        var keyboardScheme = input.FindControlScheme("Keyboard").Value;

        CreateRemappingUI(keyboardScheme);
    }
}
