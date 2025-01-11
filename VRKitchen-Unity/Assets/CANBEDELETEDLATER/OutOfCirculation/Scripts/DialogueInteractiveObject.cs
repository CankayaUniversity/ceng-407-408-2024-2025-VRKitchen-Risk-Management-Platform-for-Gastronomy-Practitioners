using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

public class DialogueInteractiveObject : BaseInteractiveObject
{
    public DialogueHandler DialogueHandler;

    //Character use different layer so they can have special lgihting applied to them, so we need to change those layer
    //that are automaticaly set. The renderers highlight both interactive object and character layers
    public override void SetLayers(bool highlighted)
    {
        RecursiveLayerSet(transform, highlighted? 14 : 13);
    }
    
    public override void Interact(NavMeshAgentController controller)
    {
        DialogueHandler.StartDialogue();
    }
}
