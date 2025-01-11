using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;


public class ConversationTSVImporter : EditorWindow
{
    const string k_ChildNamePrefix = "Phrase";
    const string k_NewTimelinePathPrefix = "Assets/Timelines/";
    const string k_NewTimelinePathPostfix = ".asset";
    
    private VisualElement m_TextImportRoot;
    private TextField m_CSVField;

    private ObjectField m_DialogueHandler;
    private List<ObjectField> m_SpeakerReferences;

    private Button m_CreateButton;

    private List<List<string>> m_ImportContent;
    
    private Dictionary<string, int> m_HeaderMapping;
    
    private Dictionary<string, DialogEntry> m_DialogEntries;
    private string m_StartStringID = "";

    private class DialogEntry
    {
        public string Speaker;
        public string Dialogue;
        public string StringID;
        public string NextPhrase;

        public AudioClip Clip;
        
        //will be null if this is not a dialog entry creating a choice
        public string[] ChoiceTargetIDs;
    }
    
    [MenuItem("Tools/TSV Importer")]
    public static void Open()
    {
        GetWindow<ConversationTSVImporter>();
    }

    private void CreateGUI()
    {
        //we create the first prompt GUI (where we can paste the TSV) child of a VisualElement so we can just remove
        //that root visual element when going into "import" mode
        m_TextImportRoot = new VisualElement();
        m_CSVField = new TextField(int.MaxValue, true, false, '*');
        m_CSVField.style.unityFont = EditorGUIUtility.Load("RobotoMono-Regular") as Font;
        
        var button = new Button();
        button.text = "Import";
        button.clicked += ImportText;

        m_TextImportRoot.Add(m_CSVField);
        m_TextImportRoot.Add(button);

        rootVisualElement.Add(m_TextImportRoot);
    }

    void ImportText()
    {
        var content = m_CSVField.text;
        if (string.IsNullOrWhiteSpace(content))
            return;

        var lines = content.Split(new []{'\r','\n'}, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 2)
            return;
        
        m_ImportContent = new List<List<string>>();
        m_HeaderMapping = new Dictionary<string, int>();
        
        //first line is headers
        var headers = lines[0].Split(new[]{'\t'});
        
        for (int i = 0; i < headers.Length; i++)
        {
            m_ImportContent.Add(new List<string>());
            m_ImportContent[i].Add(headers[i]);
            
            if(!String.IsNullOrEmpty(headers[i]))
                m_HeaderMapping.Add(headers[i], i);
        }

        //check we have all critical info
        if(!m_HeaderMapping.ContainsKey("Speaker") || !m_HeaderMapping.ContainsKey("StringID"))
        {
            Debug.LogError("Couldn't find a required column, exiting");
            return;
        }
        
        //then go for each line and fill each list with the rest of the columns
        for (int i = 1; i < lines.Length; ++i)
        {
            
            var lineContent = lines[i].Split(new[]{'\t'});
            
            //skip line that don't have a string id

            if (string.IsNullOrEmpty(lineContent[m_HeaderMapping["StringID"]]))
            {
                continue;
            }
            
            for (int l = 0; l < lineContent.Length; l++)
            {
                m_ImportContent[l].Add(lineContent[l]);   
            }
        }

        //now we have all info, let's remove the import UI and look for all reference we need to build
        rootVisualElement.Remove(m_TextImportRoot);
        
        //first let's look for the DialogHandler that will be used
        m_DialogueHandler = new ObjectField("Dialogue Handler");
        m_DialogueHandler.allowSceneObjects = true;
        m_DialogueHandler.objectType = typeof(DialogueHandler);
        m_DialogueHandler.RegisterValueChangedCallback(evt => CheckForCreationPossible());
        rootVisualElement.Add(m_DialogueHandler);

        //then we need a reference to each SubtitleIdentifier for all the different character we have
        List<string> alreadyAddedRef = new List<string>();

        int idx = m_HeaderMapping["Speaker"];
        rootVisualElement.Add(new Label("SubtitleIdentifier References"));
        m_SpeakerReferences = new List<ObjectField>();
        for (int i = 1; i < m_ImportContent[idx].Count; ++i)
        {
            var speaker = m_ImportContent[idx][i];
            if(alreadyAddedRef.Contains(speaker))
                continue;
            
            alreadyAddedRef.Add(speaker);
            var f = new ObjectField(speaker);
            f.objectType = typeof(SubtitleIdentifier);
            f.allowSceneObjects = false;
            f.RegisterValueChangedCallback(evt => CheckForCreationPossible());
            m_SpeakerReferences.Add(f);
            
            rootVisualElement.Add(f);
        }
        
        var audioFilePathContainer = new VisualElement();
        audioFilePathContainer.style.flexDirection = FlexDirection.Row;
        
        //TODO : Add Here all additional reference we may need : audio file etc.

        m_CreateButton = new Button();
        m_CreateButton.text = "Build";
        m_CreateButton.SetEnabled(false);
        m_CreateButton.clicked += () =>
        {
            if(ValidateImportData())
                CreateDialogHandler();
        };
        
        rootVisualElement.Add(m_CreateButton);
    }

    /// <summary>
    /// This is called everytime value of a reference field change. If any reference is null, we don't enable the
    /// create button as we need all info to be able to create the conversation.
    /// </summary>
    void CheckForCreationPossible()
    {
        if(m_DialogueHandler.value == null)
            return;

        foreach (var reference in m_SpeakerReferences)
        {
            if(reference.value == null)
                return;
        }

        m_CreateButton.SetEnabled(true);
    }

    /// <summary>
    /// Allow to validate we have all critical info in the TSV to create the conversation. This allow e.g. to catch
    /// missing StringID, next phrase or speakers
    /// </summary>
    /// <returns></returns>
    bool ValidateImportData()
    {
        m_DialogEntries = new Dictionary<string, DialogEntry>();
        //go over all the data and check it's in right format before we actually try to create something with it
        for (int p = 1; p < m_ImportContent[0].Count; ++p)
        {
            string stringID = m_ImportContent[m_HeaderMapping["StringID"]][p];
            string speakerKey = m_ImportContent[m_HeaderMapping["Speaker"]][p];
            string text = m_ImportContent[m_HeaderMapping["Dialogue"]][p];
            string nextPhrase = m_ImportContent[m_HeaderMapping["→ Next [phrase]"]][p];
            string audioClipName = m_ImportContent[m_HeaderMapping["Audio"]][p];

            AudioClip audioClip = null;

            if (string.IsNullOrEmpty(stringID))
            {
                Debug.LogError($"stringID for line {p} is missing, invalid data format");
                return false;
            }
            
            if (string.IsNullOrEmpty(speakerKey))
            {
                Debug.LogError($"Speaker for line {p} StringID {stringID} is missing, invalid data format");
                return false;
            }
            
            if (string.IsNullOrEmpty(nextPhrase))
            {
                Debug.LogError($"Next Phrase for line {p} StringID {stringID} is missing, invalid data format");
                return false;
            }
            
            if (p == 1)
            {
                m_StartStringID = stringID;
            }

            if (!string.IsNullOrEmpty(audioClipName))
            {
                string[] foundIDs = AssetDatabase.FindAssets(audioClipName+ " t:AudioClip");

                if (foundIDs.Length == 0)
                {
                    Debug.LogError($"Audio File {audioClipName} wasn't found for entry {stringID}");
                    return false;
                }
                
                if (foundIDs.Length > 1)
                {
                    Debug.LogError($"Multiple audio file matching name {audioClipName} found for entry {stringID} ");
                    return false;
                }
                
                audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(foundIDs[0]));
            }

            string[] choiceTargets = null;
            bool nextIsChoice = nextPhrase.Contains("Choice:");
            if (nextIsChoice)
            {
                //next entry is a choice, so extract which string ID those choices are
                var cleanEntry = nextPhrase.Replace("Choice:", "");
                var ids = cleanEntry.Split('/');

                choiceTargets = Array.Empty<string>();

                foreach (var id in ids)
                {
                    //trim as we can have leading/trailing whitespace
                    var cleanId = id.Trim();
                    ArrayUtility.Add(ref choiceTargets, cleanId);
                }

                //ensure the name is "GameObject name friendly" by removing slash as they mess with search function
                //(they are understood as hierarchy)
                nextPhrase = nextPhrase.Replace('/', '_');
                nextPhrase = nextPhrase.Replace('\\', '_');
            }
            

            m_DialogEntries.Add(stringID, new DialogEntry()
            {
                StringID = stringID,
                Speaker = speakerKey,
                Dialogue = text,
                NextPhrase = nextPhrase,
                ChoiceTargetIDs = choiceTargets,
                Clip = audioClip
            });
        }

        return true;
    }

    /// <summary>
    /// This will fill the DialogHandler given with the sentences (or try to find existing one to update them)
    /// </summary>
    void CreateDialogHandler()
    {
        var choiceManager = FindObjectOfType<MultiChoiceManager>(true);
        var subtitleManager = FindObjectOfType<SubtitleManager>(true);
        
        var dialogHandler = m_DialogueHandler.value as DialogueHandler;

        var playableDirectorRoot = dialogHandler.PlayableDirectorsRoot ?? dialogHandler.transform;

        //we clear the phases if they are some already existing. Easier than trying to find the one that may have been 
        //removed in the new version
        dialogHandler.Phrases = Array.Empty<DialogueHandler.DialoguePhrase>();;

        var speakerMapping = new Dictionary<string, SubtitleIdentifier>();
        foreach (var reference in m_SpeakerReferences)
        {
            speakerMapping.Add(reference.label, reference.value as SubtitleIdentifier);
        }

        var stringIDToPhaseIndex = new Dictionary<string, int>();

        //Starting with the first sentence of the imported dialog we will crawl from Next Phrase to Next Phrase.
        //When a next phrase contain de word Choice, then we add the choice track, and for each choice, push its next sentences
        //to process
        Stack<string> stringIdToProcess = new Stack<string>();
        stringIdToProcess.Push(m_StartStringID);

        //To allow for multiple choice to lead to same sentence (or even converging dialog tree) we make sure we do not
        //process a string ID twice so there is no duplicate
        List<string> processedStringID = new List<string>();

        while (stringIdToProcess.Count > 0)
        {
            var currentStringId = stringIdToProcess.Pop();
            processedStringID.Add(currentStringId);
            
            // slash is used as "no next sentence" so this is not a sentence and should be skipped
            if(string.IsNullOrEmpty(currentStringId) || currentStringId == "/")
               continue;

            if (!m_DialogEntries.ContainsKey(currentStringId))
            {
                Debug.LogError($"Couldn't processed requested string id {currentStringId}");
                continue;
            }
            
            var dialogEntry = m_DialogEntries[currentStringId];

            CreateOrUpdateDirector(playableDirectorRoot, dialogEntry, subtitleManager, dialogHandler, speakerMapping, stringIDToPhaseIndex);

            //check next phrase
            if (dialogEntry.ChoiceTargetIDs != null)
            { //next entry is a choice
                processedStringID.Add(dialogEntry.NextPhrase);
                
                PlayableDirector choiceDirector = null;
                MultichoiceTrack choiceTrack = null;
                AudioTrack audioTrack = null;
                AnimationTrack animTrack = null;
                    
                //first we check if we already have one existing
                var existingChoice = playableDirectorRoot.Find(dialogEntry.NextPhrase);
                if (existingChoice != null)
                {
                    choiceDirector = existingChoice.GetComponent<PlayableDirector>();

                    var timeline = choiceDirector.playableAsset as TimelineAsset;

                    foreach (var trackAsset in timeline.GetRootTracks())
                    {
                        if(trackAsset is MultichoiceTrack asset) choiceTrack = asset;
                    }
                    
                    choiceDirector.SetGenericBinding(choiceTrack, choiceManager);
                    
                    EditorUtility.SetDirty(choiceDirector);
                }
                else
                {
                    var choiceGO = new GameObject(dialogEntry.NextPhrase);
                    choiceGO.transform.SetParent(playableDirectorRoot);
                    choiceDirector = choiceGO.AddComponent<PlayableDirector>();
                    choiceDirector.playableAsset = CreateOrGetTimeline<MultichoiceTrack, MultichoiceClip>(dialogHandler, dialogEntry.StringID+"_Choice", out choiceTrack, out audioTrack, out animTrack, false, false);
                    choiceDirector.SetGenericBinding(choiceTrack, choiceManager);
                    choiceDirector.playOnAwake = false;
                }
                
                //we add a new phase that is the choice
                ArrayUtility.Add(ref dialogHandler.Phrases, new DialogueHandler.DialoguePhrase()
                {
                    Summary = dialogEntry.NextPhrase,
                    NextTimelineIndexes = Array.Empty<int>(),
                    PlayableDirector = choiceDirector
                });
                
                //we map the stringID to the index so we can retrieve it more easily to link phases together at the end
                stringIDToPhaseIndex.Add(dialogEntry.NextPhrase, dialogHandler.Phrases.Length - 1);

                if (choiceTrack == null)
                {
                    Debug.LogError($"Couldn't find a MultiChoiceTrack on existing GameObject for {dialogEntry.NextPhrase}");
                }
                else
                {

                    foreach (var clip in choiceTrack.GetClips())
                    {
                        var choiceClip = clip.asset as MultichoiceClip;

                        if (choiceClip != null)
                        {
                            //we always reset the Choice array to nothing. If this is a new creation, we need as it will be
                            //null, if this is an update, we may have different choice, so start clean.
                            choiceClip.Template.ConversationChoices.Choices = Array.Empty<MultiChoice.Choice>();

                            foreach (var choiceEntry in dialogEntry.ChoiceTargetIDs)
                            {
                                if (!m_DialogEntries.ContainsKey(choiceEntry))
                                {
                                    Debug.LogError($"Couldn't find the choice with string ID {choiceEntry} for line {dialogEntry.StringID}");
                                    continue;
                                }
                                
                                var choiceDialogEntry = m_DialogEntries[choiceEntry];

                                ArrayUtility.Add(ref choiceClip.Template.ConversationChoices.Choices,
                                    new MultiChoice.Choice()
                                    {
                                        Text = choiceDialogEntry.Dialogue,
                                        RequiredItem = null
                                    });

                                CreateOrUpdateDirector(playableDirectorRoot, choiceDialogEntry, subtitleManager, dialogHandler, speakerMapping, stringIDToPhaseIndex);
                                processedStringID.Add(choiceDialogEntry.StringID);

                                if (!processedStringID.Contains(choiceDialogEntry.NextPhrase) && !stringIdToProcess.Contains(choiceDialogEntry.NextPhrase))
                                {
                                    stringIdToProcess.Push(choiceDialogEntry.NextPhrase);
                                }
                            }
                            
                            EditorUtility.SetDirty(choiceClip);
                            EditorUtility.SetDirty(choiceDirector.playableAsset);
                        }
                    }
                }
            }
            else
            {
                //we enqueue the next phrase in the chain to be processed (if it wasn't processed already)
                if (!processedStringID.Contains(dialogEntry.NextPhrase) && !stringIdToProcess.Contains(dialogEntry.NextPhrase))
                {
                    stringIdToProcess.Push(dialogEntry.NextPhrase);
                }
            }
        }
        
        
        //we file the next timeline array for each phases
        for (int i = 0; i < dialogHandler.Phrases.Length; ++i)
        {
            string currentStringId = dialogHandler.Phrases[i].Summary;

            if (currentStringId.Contains("Choice:"))
            {
                //this phase is a choice, it's next entry array will be filled by the phase that lead to it.
                continue;
            }
            
            var dialogEntry = m_DialogEntries[currentStringId];
            

            if(stringIDToPhaseIndex.ContainsKey(dialogEntry.NextPhrase))
                ArrayUtility.Add(ref dialogHandler.Phrases[i].NextTimelineIndexes, stringIDToPhaseIndex[dialogEntry.NextPhrase]);
            else if(dialogEntry.NextPhrase != "/" && !string.IsNullOrWhiteSpace(dialogEntry.NextPhrase))
                Debug.LogWarning($"Couldn't find next phrase {dialogEntry.NextPhrase} in stringIDToPhaseIndex");
            
            if (dialogEntry.ChoiceTargetIDs != null)
            {
                //if it lead to a choice, we fill this choice phase next entry. But only if it's not already filled
                //as we may have multiple phase that lead to the same choice.

                int idx = Array.FindIndex(dialogHandler.Phrases, phase => phase.Summary == dialogEntry.NextPhrase);
                if (idx == -1)
                {
                    Debug.LogError($"Couldn't find entry in phases for {dialogEntry.NextPhrase}");
                    continue;
                }
                
                //it was already filled by another phases, no need to do it
                if(dialogHandler.Phrases[idx].NextTimelineIndexes?.Length != 0)
                    continue;

                foreach (var targetID in dialogEntry.ChoiceTargetIDs)
                {
                    var choiceEntry = m_DialogEntries[targetID];
                    
                    if(stringIDToPhaseIndex.ContainsKey(choiceEntry.StringID))
                        ArrayUtility.Add(ref dialogHandler.Phrases[idx].NextTimelineIndexes, stringIDToPhaseIndex[choiceEntry.StringID]);
                }
            }
        }
        
        //Finally, cleaning if we had existing gameobject that aren't used anymore
        List<Transform> toDestroy = new List<Transform>();
        foreach (Transform child in playableDirectorRoot)
        {
            if (!processedStringID.Contains(child.name))
            {
                toDestroy.Add(child);
            }
        }
        
        toDestroy.ForEach(t => DestroyImmediate(t.gameObject));

        EditorUtility.SetDirty(dialogHandler);
    }

    void CreateOrUpdateDirector(Transform playableDirectorRoot, DialogEntry dialogEntry, SubtitleManager subtitleManager,
        DialogueHandler dialogueHandler, Dictionary<string, SubtitleIdentifier> speakerMapping, Dictionary<string, int> stringIDToPhaseIndex)
    {
        //Check if we have an existing timeline with that name or if we need to create a new one
        Transform existingChild = playableDirectorRoot.Find(dialogEntry.StringID);
        PlayableDirector director;
        SubtitleTrack track = null;
        AudioTrack audioTrack = null;
        AnimationTrack animTrack = null;
        
        if (existingChild != null)
        {
            director = existingChild.GetComponent<PlayableDirector>();
            var timeline = director.playableAsset as TimelineAsset;
            
            RetrieveOrCreteTimelineInfo<SubtitleTrack, SubtitleClip>(timeline, out track, out audioTrack, out animTrack, true, true);

            director.SetGenericBinding(track, subtitleManager);
            EditorUtility.SetDirty(director);
        }
        else
        {
            GameObject container = new GameObject(dialogEntry.StringID);
            container.transform.SetParent(playableDirectorRoot);
            director = container.AddComponent<PlayableDirector>();
            director.playOnAwake = false;
            
            //create the timeline & add the phrase to the dialog manager
            director.playableAsset = CreateOrGetTimeline<SubtitleTrack, SubtitleClip>(dialogueHandler, dialogEntry.StringID, out track, out audioTrack, out animTrack, dialogEntry.Clip != null, true);
            //go over all lines and create a new playable director with a timeline asset and the right track in it
            director.SetGenericBinding(track, subtitleManager);

            var audioSource = dialogueHandler.GetComponentInChildren<AudioSource>();
            if (audioSource == null)
            {
                audioSource = dialogueHandler.gameObject.AddComponent<AudioSource>();
                Debug.LogWarning("An audio source was automatically added to the dialog handler, remember to set its output to voice");
            }

            director.SetGenericBinding(audioTrack, audioSource);

            EditorUtility.SetDirty(director);
        }

        if (track == null)
        {
            Debug.LogError($"Something when wrong for {dialogEntry.StringID}, couldn't find an existing Subtitle Track on already existing GameObject");
        }
        else
        {
            var identifier = speakerMapping[dialogEntry.Speaker];

            double audioDuration = 0.0;
                
            foreach (var clip in track.GetClips())
            {
                var subClip = clip.asset as SubtitleClip;

                if (subClip != null)
                {
                    subClip.Template.Subtitle.Identifier = identifier;
                    subClip.Template.Subtitle.Text = dialogEntry.Dialogue;

                    audioDuration = clip.duration;
                    
                    EditorUtility.SetDirty(director.playableAsset);
                }
            }

            if (audioTrack != null)
            {
                foreach (var clip in audioTrack.GetClips())
                {
                    var audioPlayableAsset = clip.asset as AudioPlayableAsset;
                    audioPlayableAsset.clip = dialogEntry.Clip;
                    
                    if(dialogEntry.Clip == null)
                        Debug.Log($"Dialog Entry audio clip for {dialogEntry.StringID} is null");

                    clip.duration = dialogEntry.Clip.length;
                    audioDuration = clip.duration;
                    
                    EditorUtility.SetDirty(director.playableAsset);
                }
            }
            
            if (animTrack != null)
            {
                foreach (var clip in animTrack.GetClips())
                {
                    var animClip = clip.asset as AnimationPlayableAsset;
                    animClip.clip = identifier.TalkingAnimationClip;
                    animClip.removeStartOffset = false;
                    
                    clip.duration = audioDuration;

                    EditorUtility.SetDirty(director.playableAsset);
                }
            }
        }

        ArrayUtility.Add(ref dialogueHandler.Phrases, new DialogueHandler.DialoguePhrase()
        {
            Summary = dialogEntry.StringID,
            SubtitleIdentifier = speakerMapping[dialogEntry.Speaker],
            NextTimelineIndexes = Array.Empty<int>(),
            PlayableDirector = director
        });
        
        //we map the stringID to the index so we can retrieve it more easily to link phases together at the end
        stringIDToPhaseIndex.Add(dialogEntry.StringID, dialogueHandler.Phrases.Length - 1);
    }
    
    static TimelineAsset CreateOrGetTimeline<TTrack, TClip>(DialogueHandler dialogueHandler, string StringID, out TTrack track, out AudioTrack audioTrack, out AnimationTrack animTrack, bool createAudioTrack, bool createAnimTrack) 
        where TTrack : TrackAsset, new()
        where TClip : PlayableAsset
    {
        GameObject gameObject = dialogueHandler.gameObject;
        string name =  gameObject.scene.name + "_" + gameObject.name + "_" + StringID;
        string path = k_NewTimelinePathPrefix + name + k_NewTimelinePathPostfix;

        //if there is already a timeline at this address, we return it instead of creating a new one
        var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(path);
        if (timeline != null)
        {
            RetrieveOrCreteTimelineInfo<TTrack, TClip>(timeline, out track, out audioTrack, out animTrack, createAudioTrack, createAnimTrack);

            return timeline;
        }

        TimelineAsset newTimeline = CreateInstance<TimelineAsset>();
        newTimeline.name = name;

        path = AssetDatabase.GenerateUniqueAssetPath(path);
        AssetDatabase.CreateAsset(newTimeline, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        track = newTimeline.CreateTrack<TTrack>();
        track.CreateClip<TClip>();

        if (createAnimTrack)
        {
            animTrack = newTimeline.CreateTrack<AnimationTrack>();
            animTrack.CreateClip<AnimationPlayableAsset>();
        }
        else
        {
            animTrack = null;
        }

        if (createAudioTrack)
        {
            audioTrack = newTimeline.CreateTrack<AudioTrack>();
            audioTrack.CreateClip<AudioPlayableAsset>();
        }
        else
        {
            audioTrack = null;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return newTimeline;
    }

    static void RetrieveOrCreteTimelineInfo<TTrack,TClip>(TimelineAsset timeline, out TTrack track, out AudioTrack audioTrack, out AnimationTrack animTrack,
        bool createAudioTrack, bool createAnimTrack)  
        where TTrack : TrackAsset, new()
        where TClip : PlayableAsset
    {
        track = null;
        audioTrack = null;
        animTrack = null;
            
        foreach (var trackAsset in timeline.GetRootTracks())
        {
            if (trackAsset is TTrack asset)
            {
                track = asset;
            }
            else if (trackAsset is AudioTrack audioAsset)
            {
                audioTrack = audioAsset;
            }
            else if (trackAsset is AnimationTrack animAsset)
            {
                animTrack = animAsset;
            }
        }

        if (track == null)
        {
            track = timeline.CreateTrack<TTrack>();
            track.CreateClip<TClip>();
                
            EditorUtility.SetDirty(timeline);
        }
            
        if (createAudioTrack && audioTrack == null)
        {
            audioTrack = timeline.CreateTrack<AudioTrack>();
            audioTrack.CreateClip<AudioPlayableAsset>();
                
            EditorUtility.SetDirty(timeline);
        }

        if (createAnimTrack && animTrack == null)
        {
            animTrack = timeline.CreateTrack<AnimationTrack>();
            animTrack.CreateClip<AnimationPlayableAsset>();

            EditorUtility.SetDirty(timeline);
        }
    }
}
