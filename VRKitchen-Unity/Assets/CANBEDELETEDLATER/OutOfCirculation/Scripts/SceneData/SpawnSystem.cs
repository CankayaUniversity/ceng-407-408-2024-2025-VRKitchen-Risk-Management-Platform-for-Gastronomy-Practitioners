using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnSystem
{
    private static SpawnSystem s_Instance;

    private static int s_TargetSpawnPointIdx = -1;
    
    static SpawnSystem()
    {
        s_Instance = new SpawnSystem();
    }

    public static void LoadInScene(int sceneIdx, int spawnIdx)
    {
        s_TargetSpawnPointIdx = spawnIdx;

        SceneManager.LoadScene(sceneIdx, LoadSceneMode.Single);
    }

    public static void SpawnPointNotification(SpawnPoint spawnPoint)
    {
#if UNITY_EDITOR
        //only in editor we check if we have a player. If we don't we started that scene directly, not going through
        //the start screen, so we need to initialize it. In a build that check is useless, as the player and relevant
        //object are created when starting a new game
        if (DataReference.Instance.PlayerReference == null)
        {
            DataReference.Instance.CreateUI();
            UIRoot.Instance.EnableGameUI(true);
            InitSystem.SpawnGameplayData();
        }
#endif
        
        if ((s_TargetSpawnPointIdx == -1 && spawnPoint.SpawnIndex == 0) || spawnPoint.SpawnIndex == s_TargetSpawnPointIdx)
        {
            spawnPoint.SpawnPlayerCharacterHere();
        }
    }
}
