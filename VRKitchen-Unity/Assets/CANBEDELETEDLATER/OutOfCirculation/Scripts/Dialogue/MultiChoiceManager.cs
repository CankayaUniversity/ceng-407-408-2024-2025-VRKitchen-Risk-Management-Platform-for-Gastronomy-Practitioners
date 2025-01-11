using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class MultiChoiceManager : ChoiceManager<MultiChoice>
{
    Inventory m_Inventory;
    
    void Awake()
    {
        m_Inventory = FindObjectOfType<Inventory>();
    }

    public override void SetChoices(MultiChoice choiceData)
    {
        for(int i = 0; i < choiceData.Choices.Length; ++i)
        {
            var requiredItem = choiceData.Choices[i].RequiredItem;
            if(requiredItem == null || m_Inventory.Content.Contains(requiredItem))
                UIDialogueWindow.Instance.AddChoice(choiceData.Choices[i].Text, i);
        }
    }

    public override void SetAlpha(float alpha)
    {
        // No fading used so this can be blank.
    }

#if UNITY_EDITOR
    protected override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        // UI is controlled dynamically so no properties need to be gathered.
    }
#endif
}

[Serializable]
public struct MultiChoice
{
    [Serializable]
    public struct Choice
    {
        public string Text;
        public Item RequiredItem;
    }
    
    public Choice[] Choices;
}