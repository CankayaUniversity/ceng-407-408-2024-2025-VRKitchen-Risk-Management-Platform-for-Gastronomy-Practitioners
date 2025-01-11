using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIThemePrefabButton : UIThemePrefab
{
    public Button Template;

    Image m_ButtonImage;
    
    public override void Init()
    {
        m_ButtonImage = Template.GetComponent<Image>();
    }

    public override Object GetElement(GameObject root)
    {
        return root.GetComponent<Button>();
    }

    public override void Apply(Object uiElement)
    {
        Button target = uiElement as Button;
        
        if(target == null) return;
        
        target.GetComponent<Image>().sprite = m_ButtonImage.sprite;
        target.spriteState = Template.spriteState;
        target.colors = Template.colors;
        
        if (Template.transition == Selectable.Transition.SpriteSwap)
        {
            target.colors = Template.colors;
        }
        else if(Template.transition == Selectable.Transition.SpriteSwap)
        {
            target.spriteState = Template.spriteState;
        }
    }
}
