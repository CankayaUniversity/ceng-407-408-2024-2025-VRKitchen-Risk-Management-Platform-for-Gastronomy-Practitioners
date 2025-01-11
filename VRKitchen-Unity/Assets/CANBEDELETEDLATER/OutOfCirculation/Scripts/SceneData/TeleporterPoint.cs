using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleporterPoint : BaseInteractiveObject
{
    public int TargetSceneIndex;
    public int TargetSpawnPointIndex = 0;

    public override void Interact(NavMeshAgentController controller)
    {
        base.Interact(controller);
        SpawnSystem.LoadInScene(TargetSceneIndex, TargetSpawnPointIndex);
    }
}
