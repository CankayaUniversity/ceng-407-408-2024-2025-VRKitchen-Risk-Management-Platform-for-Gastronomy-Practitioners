using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIThemePrefabText : UIThemePrefab
{
    public TextMeshProUGUI TextPrefab;
    
    public override void Apply(Object uiElement)
    {
        TextMeshProUGUI target = uiElement as TextMeshProUGUI;

        if (target == null) return;
        
        target.font = TextPrefab.font;
        target.fontSize = TextPrefab.fontSize;
        target.fontSizeMax = TextPrefab.fontSize;
        target.color = TextPrefab.color;
    }
    
    public override Object GetElement(GameObject root)
    {
        return root.GetComponent<TextMeshProUGUI>();
    }
}
