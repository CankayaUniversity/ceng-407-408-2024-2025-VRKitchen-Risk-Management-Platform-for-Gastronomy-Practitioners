using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

[Serializable]
public class MultichoiceBehaviour : PlayableBehaviour
{
    public MultiChoice ConversationChoices;
}
