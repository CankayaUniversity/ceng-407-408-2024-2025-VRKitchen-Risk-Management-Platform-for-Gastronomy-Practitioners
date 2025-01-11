using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class MultichoiceMixerBehaviour : PlayableBehaviour
{
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        //we exit if the application isn't playing, otherwise scrubbing the timeline in the editor will try to create
        //choices...
        if(!Application.isPlaying)
            return;
        
        BaseChoiceManager trackBinding = playerData as BaseChoiceManager;

        if (trackBinding == null)
            return;

        int inputCount = playable.GetInputCount ();

        float totalWeight = 0f;
        float greatestWeight = 0f;

        if (trackBinding is MultiChoiceManager)
        {
            MultiChoiceManager conversationChoiceManager = playerData as MultiChoiceManager;
            
            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);

                ScriptPlayable<MultichoiceBehaviour> inputPlayable = (ScriptPlayable<MultichoiceBehaviour>)playable.GetInput(i);
                MultichoiceBehaviour input = inputPlayable.GetBehaviour ();
            
                totalWeight += inputWeight;
                if (inputWeight > greatestWeight)
                {
                    greatestWeight = inputWeight;
                    conversationChoiceManager.SetChoices(input.ConversationChoices);
                }
            
                conversationChoiceManager.SetAlpha(totalWeight);
            }
        }
    }
}