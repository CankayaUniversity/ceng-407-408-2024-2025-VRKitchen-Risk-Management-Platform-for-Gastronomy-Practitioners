using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGamepadControlSettingScreen : UIControlSettingBase
{
    public override void Build()
    {
        base.Build();
        
        var input = ControlManager.CurrentInput;
        
        //This is the keyboard option screen so we only get the Mouse/Keyboard Scheme
        var gamepadScheme = input.FindControlScheme("Gamepad").Value;
        
        CreateRemappingUI(gamepadScheme);
    }
}
