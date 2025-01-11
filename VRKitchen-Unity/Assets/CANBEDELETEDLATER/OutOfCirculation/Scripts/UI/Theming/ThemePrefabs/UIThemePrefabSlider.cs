using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIThemePrefabSlider : UIThemePrefab
{
    public Slider SliderPrefab;

    Image m_HandleImage;
    Image m_SliderBackground;
    
    public override void Init()
    {
        m_HandleImage = SliderPrefab.targetGraphic as Image;
        m_SliderBackground = SliderPrefab.transform.Find("Background").GetComponentInChildren<Image>(true);
    }
    
    public override Object GetElement(GameObject root)
    {
        return root.GetComponent<Slider>();
    }

    public override void Apply(Object uiElement)
    {
        Slider target = uiElement as Slider;
        
        if(target == null) return;
        
        Image img = target.targetGraphic as Image;
            
        img.sprite = m_HandleImage.sprite;
        img.color = m_HandleImage.color;
        img.type = img.sprite.border.magnitude > 0.001f ? Image.Type.Sliced : Image.Type.Simple;

        var bg = target.transform.Find("Background").GetComponentInChildren<Image>(true);
        bg.sprite = m_SliderBackground.sprite;
        bg.type =  bg.sprite.border.magnitude > 0.001f ? Image.Type.Sliced : Image.Type.Simple;

        if (SliderPrefab.transition == Selectable.Transition.SpriteSwap)
        {
            target.colors = SliderPrefab.colors;
        }
        else if(SliderPrefab.transition == Selectable.Transition.SpriteSwap)
        {
            target.spriteState = SliderPrefab.spriteState;
        }
    }
}
