using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class SpawnPoint : MonoBehaviour
{
    public event Action<SpawnPoint, NavMeshAgentController> OnPlayerCharacterSpawn
    {
        add
        {
            m_OnPlayerCharacterSpawn += value;

            //if the spawn already happen, we notify right away
            if (m_SpawnHappened) value(this, DataReference.Instance.PlayerReference);
        }
        remove => m_OnPlayerCharacterSpawn -= value;
    }
    
    public int SpawnIndex = 0;

    public UnityEvent OnSpawnEvent;

    private bool m_SpawnHappened;
    private event Action<SpawnPoint, NavMeshAgentController> m_OnPlayerCharacterSpawn;


    private void Awake()
    {
        m_SpawnHappened = false;
        SpawnSystem.SpawnPointNotification(this);
    }

    public void SpawnPlayerCharacterHere()
    {
        DataReference.Instance.PlayerReference.NavMeshAgent.Warp(transform.position);
        DataReference.Instance.PlayerReference.gameObject.SetActive(true);
        
        OnSpawnEvent?.Invoke();
        
        if(m_OnPlayerCharacterSpawn != null)
            m_OnPlayerCharacterSpawn.Invoke(this, DataReference.Instance.PlayerReference);

        m_SpawnHappened = true;
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        //check our spawn index is unique, otherwise, make it unique
        var allSpawnPoints = FindObjectsOfType<SpawnPoint>(true);

        Array.Sort(allSpawnPoints, (point, spawnPoint) => point.SpawnIndex.CompareTo(spawnPoint.SpawnIndex) );

        //all spawnPoint are ordered in increasing index, so we just need to push the index till it is not equal anymore
        //to find a "gap" in the spawnPoint index numbering, or reach the end + 1.
        foreach (var spawnPoint in allSpawnPoints)
        {
            if (spawnPoint != this)
            {
                if (spawnPoint.SpawnIndex == this.SpawnIndex)
                {
                    SpawnIndex += 1;
                    EditorUtility.SetDirty(this);
                }
            }
        }
    }
#endif
}

#if UNITY_EDITOR

[CustomEditor(typeof(SpawnPoint))]
public class SpawnPointEditor : Editor
{
    private SerializedProperty m_IndexProperty;
    private SerializedProperty m_OnSpawnEventProperty;

    private void OnEnable()
    {
        m_IndexProperty = serializedObject.FindProperty(nameof(SpawnPoint.SpawnIndex));
        m_OnSpawnEventProperty = serializedObject.FindProperty(nameof(SpawnPoint.OnSpawnEvent));
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUI.enabled = false;
        EditorGUILayout.PropertyField(m_IndexProperty);
        GUI.enabled = true;

        EditorGUILayout.PropertyField(m_OnSpawnEventProperty);
        
        serializedObject.ApplyModifiedProperties();
    }

}

#endif
