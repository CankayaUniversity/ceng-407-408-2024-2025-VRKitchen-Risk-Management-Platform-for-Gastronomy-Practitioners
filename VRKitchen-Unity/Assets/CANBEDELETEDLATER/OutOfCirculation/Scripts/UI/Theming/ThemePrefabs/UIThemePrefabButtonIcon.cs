using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIThemePrefabButtonIcon : UIThemePrefabButton
{
    public Image imagePrefab;
    public bool syncImage = false;
    public bool syncColor = false;
    
    public override void Apply(Object uiElement)
    {
        base.Apply(uiElement);
        
        Button target = uiElement as Button;
        
        if(target == null) return;

        foreach (Transform child in target.transform)
        {
            var targetIcon = child.GetComponent<Image>();
            
            if(targetIcon == null)
                continue;

            if (syncImage) targetIcon.sprite = imagePrefab.sprite;
            if (syncColor) targetIcon.color = imagePrefab.color;
        }
        
    }
}
