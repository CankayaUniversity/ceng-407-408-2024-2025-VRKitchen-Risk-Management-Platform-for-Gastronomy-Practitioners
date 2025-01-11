using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class SubtitleIdentifier : ScriptableObject
{
    public Sprite Portrait;
    public Color TextColor = Color.black;
    public String SpeakerName = "Name";
    
    public AnimationClip TalkingAnimationClip;
}
