using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[CustomEditor(typeof(DialogueHandler))]
public class DialogHandlerEditor : Editor
{
    SerializedProperty m_CharacterProp;
    private SerializedProperty m_RootProperty;
    SerializedProperty m_CameraControllerProp;
    SerializedProperty m_PhrasesProp;
    DialogueHandler m_DialogueHandler;
    Transform m_DialogHandlerTransform;
    SubtitleManager m_SubtitleManager;

    static readonly GUIContent k_PhrasesTitle = new GUIContent("Phrases");
    static readonly GUIContent k_AddPhraseButtonLabel = new GUIContent("Add Phrase");
    static readonly GUIContent k_RemovePhraseButtonLabel = new GUIContent("Remove Phrase");
    static readonly GUIContent k_AutoSetupPhraseButtonLabel = new GUIContent("Auto Setup Phrase");

    static readonly GUILayoutOption[] k_ButtonLayoutOptions = { GUILayout.Width(110f) };
    
    const string k_ChildNamePrefix = "Phrase";
    const string k_NewTimelinePathPrefix = "Assets/Timelines/";
    const string k_NewTimelinePathPostfix = ".asset";

    void OnEnable()
    {
        m_CharacterProp = serializedObject.FindProperty(nameof(DialogueHandler.Character));
        m_RootProperty = serializedObject.FindProperty(nameof(DialogueHandler.PlayableDirectorsRoot));
        m_CameraControllerProp = serializedObject.FindProperty(nameof(DialogueHandler.CameraController));
        m_PhrasesProp = serializedObject.FindProperty(nameof(DialogueHandler.Phrases));
        m_DialogueHandler = (DialogueHandler)target;
        m_DialogHandlerTransform = m_DialogueHandler.transform;
        m_SubtitleManager = FindObjectOfType<SubtitleManager>();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_CharacterProp);
        EditorGUILayout.PropertyField(m_RootProperty);
        EditorGUILayout.PropertyField(m_CameraControllerProp);
        EditorGUILayout.LabelField(k_PhrasesTitle);
        EditorGUI.indentLevel++;

        for (int i = 0; i < m_PhrasesProp.arraySize; i++)
        {
            SerializedProperty phraseProp = m_PhrasesProp.GetArrayElementAtIndex(i);
            string summary = phraseProp.FindPropertyRelative(nameof(DialogueHandler.DialoguePhrase.Summary)).stringValue;
            string foldoutLabel = "ID: " + i;
            if (!string.IsNullOrEmpty(summary))
                foldoutLabel += "\t\t" + summary;
            phraseProp.isExpanded = EditorGUILayout.Foldout(phraseProp.isExpanded, foldoutLabel);
            if (phraseProp.isExpanded && DisplayPhrase(phraseProp, i))
            {
                RemovePhrase(i);
                i--;
            }
        }
        
        EditorGUI.indentLevel--;
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(k_AddPhraseButtonLabel, k_ButtonLayoutOptions))
        {
            AddPhrase();
        }
        EditorGUILayout.EndHorizontal();
        
        serializedObject.ApplyModifiedProperties();
    }

    bool DisplayPhrase(SerializedProperty phraseProp, int index)
    {
        bool doRemove = false;
        SerializedProperty summaryProp = phraseProp.FindPropertyRelative(nameof(DialogueHandler.DialoguePhrase.Summary));
        SerializedProperty playableDirectorProp = phraseProp.FindPropertyRelative(nameof(DialogueHandler.DialoguePhrase.PlayableDirector));
        SerializedProperty subtitleIdentifierProp = phraseProp.FindPropertyRelative(nameof(DialogueHandler.DialoguePhrase.SubtitleIdentifier));
        SerializedProperty nextTimelineIndexesProp = phraseProp.FindPropertyRelative(nameof(DialogueHandler.DialoguePhrase.NextTimelineIndexes));

        EditorGUI.indentLevel++;

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.PropertyField(summaryProp);
        EditorGUILayout.PropertyField(playableDirectorProp);
        EditorGUILayout.PropertyField(subtitleIdentifierProp);
        EditorGUILayout.PropertyField(nextTimelineIndexesProp);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(k_AutoSetupPhraseButtonLabel, k_ButtonLayoutOptions))
        {
            AutoSetup(phraseProp, index);
        }
        if (GUILayout.Button(k_RemovePhraseButtonLabel, k_ButtonLayoutOptions))
        {
            doRemove = true;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        
        EditorGUI.indentLevel--;
        return doRemove;
    }

    void AddPhrase()
    {
        int newPhraseIndex = m_PhrasesProp.arraySize;
        m_PhrasesProp.arraySize++;
        SerializedProperty newPhraseProp = m_PhrasesProp.GetArrayElementAtIndex(newPhraseIndex);

        SerializedProperty summaryProp = newPhraseProp.FindPropertyRelative(nameof(DialogueHandler.DialoguePhrase.Summary));
        SerializedProperty playableDirectorProp = newPhraseProp.FindPropertyRelative(nameof(DialogueHandler.DialoguePhrase.PlayableDirector));
        SerializedProperty nextTimelineIndexesProp = newPhraseProp.FindPropertyRelative(nameof(DialogueHandler.DialoguePhrase.NextTimelineIndexes));

        summaryProp.stringValue = "";
        playableDirectorProp.objectReferenceValue = null;
        nextTimelineIndexesProp.arraySize = 0;
    }

    void RemovePhrase(int phraseIndex)
    {
        // TODO: discuss how much should be removed by this? should a child gameobject, should a timeline?
        m_PhrasesProp.DeleteArrayElementAtIndex(phraseIndex);

        for (int i = phraseIndex + 1; i < m_PhrasesProp.arraySize; i++)
        {
            Transform child = m_DialogHandlerTransform.Find(k_ChildNamePrefix + i);

            if (child != null && child.name == k_ChildNamePrefix + i)
            {
                child.name = k_ChildNamePrefix + (i - 1);
            }
        }
        
        Transform childToBeDestroyed = m_DialogHandlerTransform.Find(k_ChildNamePrefix + phraseIndex);
        if(childToBeDestroyed != null)
            Destroy(childToBeDestroyed);

        for (int i = 0; i < m_PhrasesProp.arraySize; i++)
        {
            SerializedProperty phraseProp = m_PhrasesProp.GetArrayElementAtIndex(i);
            
            SerializedProperty nextTimelineIndexesProp = phraseProp.FindPropertyRelative(nameof(DialogueHandler.DialoguePhrase.NextTimelineIndexes));

            for (int j = 0; j < nextTimelineIndexesProp.arraySize; j++)
            {
                SerializedProperty nextTimelineIndexProp = nextTimelineIndexesProp.GetArrayElementAtIndex(i);

                if (nextTimelineIndexProp.intValue == phraseIndex)
                {
                    nextTimelineIndexesProp.DeleteArrayElementAtIndex(j);
                    j--;
                }
                else if (nextTimelineIndexProp.intValue > phraseIndex)
                {
                    nextTimelineIndexProp.intValue--;
                }
            }
        }
    }

    void AutoSetup(SerializedProperty phraseProp, int index)
    {
        SerializedProperty playableDirectorProp = phraseProp.FindPropertyRelative(nameof(DialogueHandler.DialoguePhrase.PlayableDirector));
        Transform child = m_DialogHandlerTransform.Find(k_ChildNamePrefix + index);
        PlayableDirector playableDirector;
        
        if(playableDirectorProp.objectReferenceValue != null)
        {
            playableDirector = playableDirectorProp.objectReferenceValue as PlayableDirector;

            if (playableDirector.playableAsset == null)
            {
                playableDirector.playableAsset = CreateTimelineAsset(m_DialogueHandler, index, out SubtitleTrack subtitleTrack);
                playableDirector.SetGenericBinding(subtitleTrack, m_SubtitleManager);
            }
        }
        else
        {
            if (child == null)
            {
                GameObject newChild = new GameObject(k_ChildNamePrefix + index);
                newChild.transform.parent = m_DialogHandlerTransform;
                child = newChild.transform;
            }
            
            playableDirector = child.GetComponent<PlayableDirector>();

            if (playableDirector == null)
                playableDirector = child.gameObject.AddComponent<PlayableDirector>();

            playableDirectorProp.objectReferenceValue = playableDirector;

            if (playableDirector.playableAsset == null)
            {
                playableDirector.playableAsset = CreateTimelineAsset(m_DialogueHandler, index, out SubtitleTrack subtitleTrack);
                playableDirector.SetGenericBinding(subtitleTrack, m_SubtitleManager);
            }
        }

        playableDirector.playOnAwake = false;
        
        SerializedProperty nextTimelineIndexesProp = phraseProp.FindPropertyRelative(nameof(DialogueHandler.DialoguePhrase.NextTimelineIndexes));
        if (m_PhrasesProp.arraySize != index + 1 && nextTimelineIndexesProp.arraySize == 0)
        {
            nextTimelineIndexesProp.arraySize++;
            nextTimelineIndexesProp.GetArrayElementAtIndex(0).intValue = index + 1;
        }
    }

    static TimelineAsset CreateTimelineAsset(DialogueHandler dialogueHandler, int index, out SubtitleTrack subtitleTrack)
    {
        TimelineAsset newTimeline = CreateInstance<TimelineAsset>();
        GameObject gameObject = dialogueHandler.gameObject;
        newTimeline.name = gameObject.scene.name + "_" + gameObject.name + "_Phrase" + index;
        string path = k_NewTimelinePathPrefix + newTimeline.name + k_NewTimelinePathPostfix;
        path = AssetDatabase.GenerateUniqueAssetPath(path);
        AssetDatabase.CreateAsset(newTimeline, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        subtitleTrack = newTimeline.CreateTrack<SubtitleTrack>();
        subtitleTrack.CreateClip<SubtitleClip>();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return newTimeline;
    }
}
