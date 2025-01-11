using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public abstract class BaseChoiceManager : MonoBehaviour
{
    public abstract void SetAlpha(float alpha);
    
#if UNITY_EDITOR
    public static void GatherProperties(MultichoiceTrack choiceTrack, PlayableDirector director, IPropertyCollector driver)
    {
        BaseChoiceManager conversationChoiceManager = director.GetGenericBinding(choiceTrack) as BaseChoiceManager;
        conversationChoiceManager.GatherProperties(director, driver);
    }
    
    protected abstract void GatherProperties(PlayableDirector director, IPropertyCollector driver);
#endif
}
