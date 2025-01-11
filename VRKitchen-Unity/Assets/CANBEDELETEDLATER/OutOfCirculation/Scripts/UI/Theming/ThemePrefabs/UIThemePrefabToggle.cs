using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using Toggle = UnityEngine.UI.Toggle;

public class UIThemePrefabToggle : UIThemePrefab
{
    public Toggle template;

    Image m_BackgroundImage;
    Image m_CheckImage;

    public override void Init()
    {
        m_BackgroundImage = template.targetGraphic as Image;
        m_CheckImage = template.graphic as Image;
    }
    
    public override Object GetElement(GameObject root)
    {
        return root.GetComponent<Toggle>();
    }

    public override void Apply(Object uiElement)
    {
        Toggle target = uiElement as Toggle;
        
        if(target == null) return;

        var background = target.targetGraphic as Image;
        var check = target.graphic as Image;

        background.sprite = m_BackgroundImage.sprite;
        background.color = m_BackgroundImage.color;
        
        check.sprite = m_CheckImage.sprite;
        check.color = m_CheckImage.color;

        target.transition = template.transition;
        
        if (template.transition == Selectable.Transition.ColorTint)
        {
            target.colors = template.colors;
        }
        else if(template.transition == Selectable.Transition.SpriteSwap)
        {
            target.spriteState = template.spriteState;
        }
    }
}
