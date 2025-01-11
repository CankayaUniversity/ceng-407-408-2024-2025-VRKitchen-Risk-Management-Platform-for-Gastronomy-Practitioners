using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Subtitle : IComparable<Subtitle>
{
    public SubtitleIdentifier Identifier;
    public string Text = "[default subtitle]";
    public AudioClip AudioClip;
    public int Priority = 1;
    public bool IsLogged = true;
    [HideInInspector]
    public float Weight;

    public int CompareTo(Subtitle other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        int priorityComparison = Priority.CompareTo(other.Priority);
        if (priorityComparison != 0)
            return priorityComparison;
        return Weight.CompareTo(other.Weight);
    }

    public void SetupLogEntry(RectTransform logEntry)
    {
        // TODO: find components and set them up appropriately.
        // TODO: resize logEntry appropriately.
    }

    //return the string including the speaker name
    public string GetDialogueText()
    {
        return $"<b>{Identifier.SpeakerName}:</b> {Text}";
    }
}