using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class ConversationChoiceManager : ChoiceManager<ConversationChoices>
{
    [Serializable]
    public struct ConversationChoiceUI
    {
        public CanvasGroup canvasGroup;
        public Image icon;
        public TextMeshProUGUI text;
    }
    
    public ConversationChoiceUI choice0;
    public ConversationChoiceUI choice1;
    public ConversationChoiceUI choice2;
    public ConversationChoiceUI choice3;
    
    public override void SetChoices(ConversationChoices choiceData)
    {
        choice0.icon.sprite = choiceData.subtitleIdentifier.Portrait;
        choice1.icon.sprite = choiceData.subtitleIdentifier.Portrait;
        choice2.icon.sprite = choiceData.subtitleIdentifier.Portrait;
        choice3.icon.sprite = choiceData.subtitleIdentifier.Portrait;

        choice0.text.color = choiceData.subtitleIdentifier.TextColor;
        choice1.text.color = choiceData.subtitleIdentifier.TextColor;
        choice2.text.color = choiceData.subtitleIdentifier.TextColor;
        choice3.text.color = choiceData.subtitleIdentifier.TextColor;

        choice0.text.text = choiceData.option0;
        choice1.text.text = choiceData.option1;
        choice2.text.text = choiceData.option2;
        choice3.text.text = choiceData.option3;
    }

    public override void SetAlpha(float alpha)
    {
        choice0.canvasGroup.alpha = alpha;
        choice1.canvasGroup.alpha = alpha;
        choice2.canvasGroup.alpha = alpha;
        choice3.canvasGroup.alpha = alpha;
    }

#if UNITY_EDITOR
    protected override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        driver.AddFromName(choice0.canvasGroup, "m_Alpha");
        driver.AddFromName(choice0.text, "m_text");
        driver.AddFromName(choice0.text, "m_fontColor");
        driver.AddFromName(choice0.icon, "m_Sprite");
        
        driver.AddFromName(choice1.canvasGroup, "m_Alpha");
        driver.AddFromName(choice1.text, "m_text");
        driver.AddFromName(choice1.text, "m_fontColor");
        driver.AddFromName(choice1.icon, "m_Sprite");
        
        driver.AddFromName(choice2.canvasGroup, "m_Alpha");
        driver.AddFromName(choice2.text, "m_text");
        driver.AddFromName(choice2.text, "m_fontColor");
        driver.AddFromName(choice2.icon, "m_Sprite");
        
        driver.AddFromName(choice3.canvasGroup, "m_Alpha");
        driver.AddFromName(choice3.text, "m_text");
        driver.AddFromName(choice3.text, "m_fontColor");
        driver.AddFromName(choice3.icon, "m_Sprite");
    }
#endif
}
