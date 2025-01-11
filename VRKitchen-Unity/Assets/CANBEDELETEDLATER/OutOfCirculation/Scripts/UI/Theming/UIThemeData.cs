using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "UIThemePrefabList", menuName = "Accessibility/UI Theme Prefab List")]
public class UIThemeData : ScriptableObject, ISerializationCallbackReceiver
{
    [Serializable]
    public class KeywordEntry
    {
        public string Key;
        public UIThemePrefab Prefab;
    }
    
    public UIThemePrefabButton ButtonPrefab;
    public UIThemePrefabSlider SliderPrefab;
    public UIThemePrefabText TextPrefab;
    public UIThemePrefabToggle TogglePrefab;

    public Image BackgroundPrefab;

    public KeywordEntry[] SpecialRules;

    protected Dictionary<string, KeywordEntry> m_RulesLookup;

    public void ApplyThemeToHierarchy(Transform root)
    {
        // Note : some of those are not very efficient (use of GetComponent etc.) but as this is an operation that will
        // be done quite rarely, we can afford a little slowdown for the sake of keeping the code and other manual
        // setup of reference in editor to a simpler level for that vertical slice.
        
        TextPrefab.Init();
        var allLabels = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var label in allLabels)
        {
            if(label.GetComponent<UIThemeSpecialRule>() != null)
                continue;
            
            TextPrefab.Apply(label);
        }
        
        
        ButtonPrefab.Init();
        var allButtons = root.GetComponentsInChildren<Button>(true);
        foreach (var button in allButtons)
        {
            //Special rules will be handled later
            if(button.GetComponentInParent<UIThemeSpecialRule>() != null)
                continue;
            
            ButtonPrefab.Apply(button);
        }

        var allBGs = root.GetComponentsInChildren<UIThemeBackground>(true);
        foreach (var bg in allBGs)
        {
            if(bg.GetComponentInParent<UIThemeSpecialRule>() != null)
                continue;
            
            bg.GetComponent<Image>().sprite = BackgroundPrefab.sprite;
        }

        SliderPrefab.Init();
        var allSliders = root.GetComponentsInChildren<Slider>(true);
        foreach (var slider in allSliders)
        {
            if(slider.GetComponentInParent<UIThemeSpecialRule>() != null)
                continue;
            
            SliderPrefab.Apply(slider);
        }
        
        TogglePrefab.Init();
        var allToggle = root.GetComponentsInChildren<Toggle>(true);
        foreach (var toggle in allToggle)
        {
            if(toggle.GetComponentInParent<UIThemeSpecialRule>() != null)
                continue;
            
            TogglePrefab.Apply(toggle);
        }
        
        //now handle all the special rules entries
        var allSpecialRules = root.GetComponentsInChildren<UIThemeSpecialRule>();
        foreach (var specialRule in allSpecialRules)
        {
            if (m_RulesLookup.TryGetValue(specialRule.RuleName, out var ruleEntry))
            {
                ruleEntry.Prefab.Init();

                var uiElement = ruleEntry.Prefab.GetElement(specialRule.gameObject);
                ruleEntry.Prefab.Apply(uiElement);
            }
        }
    }

    public void OnBeforeSerialize()
    {
        
    }

    public void OnAfterDeserialize()
    {
        m_RulesLookup = new Dictionary<string, KeywordEntry>();
        
        if(SpecialRules == null)
            return;

        foreach (var rule in SpecialRules)
        {
            m_RulesLookup.Add(rule.Key, rule);
        }
    }
}
