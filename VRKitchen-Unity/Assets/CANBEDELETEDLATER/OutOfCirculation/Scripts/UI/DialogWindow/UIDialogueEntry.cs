using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDialogueEntry : MonoBehaviour
{
    public RectTransform RectTransform;
    public TextMeshProUGUI SubtitleEntry;

    public RectTransform IconeContainer;
    public Image Icone;

    public void Setup(Subtitle subtitle)
    {
        SubtitleEntry.text = subtitle.GetDialogueText();
        SubtitleEntry.color = subtitle.Identifier.TextColor;
        Icone.sprite = subtitle.Identifier.Portrait;
    }
}
