using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIThemePrefabImage : UIThemePrefab
{
    public Image imagePrefab;
    public bool syncColor = true;
    
    public override void Apply(Object uiElement)
    {
        var target = uiElement as Image;
        
        if(target == null) return;

        target.sprite = imagePrefab.sprite;
        if (syncColor) target.color = imagePrefab.color;
    }

    public override Object GetElement(GameObject root)
    {
        return root.GetComponent<Image>();
    }
}
