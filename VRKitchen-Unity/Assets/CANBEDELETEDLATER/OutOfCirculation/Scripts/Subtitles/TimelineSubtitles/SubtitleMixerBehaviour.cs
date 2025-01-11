using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class SubtitleMixerBehaviour : PlayableBehaviour
{
    SubtitleManager m_SubtitleManager;
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        //avoid doing all this when scrubbing in the timeline editor 
        if(!Application.isPlaying)
            return;
        
        SubtitleManager trackBinding = playerData as SubtitleManager;

        if (trackBinding == null)
            return;

        if (m_SubtitleManager == null)
            m_SubtitleManager = trackBinding;

        int inputCount = playable.GetInputCount ();

        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<SubtitleBehaviour> inputPlayable = (ScriptPlayable<SubtitleBehaviour>)playable.GetInput(i);
            SubtitleBehaviour input = inputPlayable.GetBehaviour ();
            
            if(inputWeight > 0f)
                m_SubtitleManager.AddToCurrentSubtitles(input.Subtitle);
            else
                m_SubtitleManager.RemoveFromCurrentSubtitles(input.Subtitle);
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        //avoid doing all this when scrubbing in the timeline editor 
        if(!Application.isPlaying)
            return;
    
        //if the state is Playing, that mean it actually "stopped" so reach the end, not Paused
        if (info.effectivePlayState == PlayState.Playing)
        {
            int inputCount = playable.GetInputCount();

            for (int i = 0; i < inputCount; i++)
            {
                ScriptPlayable<SubtitleBehaviour> inputPlayable =
                    (ScriptPlayable<SubtitleBehaviour>)playable.GetInput(i);

                SubtitleBehaviour input = inputPlayable.GetBehaviour();

                m_SubtitleManager.RemoveFromCurrentSubtitles(input.Subtitle);
            }
        }
    }
}