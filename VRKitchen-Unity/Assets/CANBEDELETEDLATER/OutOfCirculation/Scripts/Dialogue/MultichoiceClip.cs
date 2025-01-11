using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

[Serializable]
public class MultichoiceClip : PlayableAsset, ITimelineClipAsset
{
    public MultichoiceBehaviour Template = new MultichoiceBehaviour ();

    //force length to just one second, there is no audio to play there, the clip is just use to push the choice to the
    //dialogue windows.
    public override double duration => 1.0f;

    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<MultichoiceBehaviour>.Create (graph, Template);
        return playable;
    }
}
