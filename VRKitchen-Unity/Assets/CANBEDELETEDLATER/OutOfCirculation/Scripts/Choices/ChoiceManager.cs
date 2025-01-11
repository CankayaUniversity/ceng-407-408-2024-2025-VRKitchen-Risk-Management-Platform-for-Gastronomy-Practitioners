using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public abstract class ChoiceManager<TChoiceData> : BaseChoiceManager
{
    public abstract void SetChoices(TChoiceData choiceData);
}
