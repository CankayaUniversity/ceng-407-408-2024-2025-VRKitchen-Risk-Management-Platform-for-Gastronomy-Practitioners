using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

[Serializable]
public class SubtitleClip : PlayableAsset, ITimelineClipAsset
{
    public SubtitleBehaviour Template = new SubtitleBehaviour ();

    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<SubtitleBehaviour>.Create (graph, Template);
        
        return playable;
    }

    //TODO : link that to the audio clip? As we seem to go for 1 sentence (so audio clip), 1 subtitle, the audio clip will
    //drive the timeline length, so having that at 1s all the time shouldn't be a problem?
    public override double duration { get => 1.0f; }
}
