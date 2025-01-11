using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

/// <summary>
/// Handle everything related to dialog. Classic use is adding that to a DialogInteractiveObject that will call this
/// class StartDialog when interacted with. 
/// </summary>
public class DialogueHandler : MonoBehaviour
{
    private const int k_NoIndex = -1;
    private const int k_NextIsChoice = -2;
    
    [Serializable]
    public struct DialoguePhrase
    {
        public string Summary;
        public PlayableDirector PlayableDirector;
        public SubtitleIdentifier SubtitleIdentifier;
        public int[] NextTimelineIndexes;
    }
    
    public SubtitleIdentifier Character;
    public Transform PlayableDirectorsRoot;
    public CameraController CameraController;
    public DialoguePhrase[] Phrases;

    public bool IsPlaying => m_IsPlaying;
    
    int m_CurrentIndex;
    int m_NextIndex;

    bool m_CanBeNexted = true;
    bool m_IsPlaying;

    PlayableDirector m_CurrentPlayingDirector;
    
    void Start()
    {
        Animator ownController = GetComponentInChildren<Animator>();
        AudioSource ownSource = GetComponentInChildren<AudioSource>();
        //TODO : maybe us a direct reference here instead of GetComponent
        Animator playerController =
            DataReference.Instance.PlayerReference.GetComponentInChildren<Animator>();
        AudioSource playerSource = playerController.GetComponentInChildren<AudioSource>();
        
        foreach (DialoguePhrase timeline in Phrases)
        {
            timeline.PlayableDirector.played += director =>
            {
                m_CurrentPlayingDirector = director;
            };
            
            timeline.PlayableDirector.stopped += director =>
            {
                m_CanBeNexted = true;
                timeline.PlayableDirector.time = 0;

                
                if (m_NextIndex == k_NextIsChoice)
                {//if it's a choice, activate the choice entry 
                    UIDialogueWindow.Instance.ActivateChoices();
                }
                else if(m_NextIndex == k_NoIndex || SaveSystem.CurrentSettings.PauseDialogue)
                {
                    //if we are at the last sentence or pause dialogue is one, we display the next prompt to notify to the
                    //user that they need to press a button to continue 
                    UIDialogueWindow.Instance.DisplayNextPrompt(true);
                }
                else
                {
                    //finally if there is a next setence and pause dialogue is off, we just continue
                    PlayNext();
                }
            };
            
            //TODO : store the anim track index somewhere? Timeline doesn't make it easy to dynamically bind the controller
            var timelineAsset = timeline.PlayableDirector.playableAsset as TimelineAsset;
            AnimationTrack animTrack = null;
            AudioTrack audioTrack = null;
            foreach (var track in timelineAsset.GetRootTracks())
            {
                if (track is AnimationTrack animTrackRef)
                    animTrack = animTrackRef;

                if (track is AudioTrack audioTrackRef)
                    audioTrack = audioTrackRef;
            }

            if (animTrack != null)
            {
                //we assign the controllers
                //TODO : rely on something more fixed than simply name. Bool on the indentifier to tag as Player?
                if (timeline.SubtitleIdentifier.SpeakerName == "Sureswim")
                {
                    timeline.PlayableDirector.SetGenericBinding(animTrack, playerController);
                    timeline.PlayableDirector.SetGenericBinding(audioTrack, playerSource);
                }
                else
                {
                    timeline.PlayableDirector.SetGenericBinding(animTrack, ownController);
                    timeline.PlayableDirector.SetGenericBinding(audioTrack, ownSource);
                }
            }
        }
        
        InputActionAsset inputReference = ControlManager.CurrentInput;

        inputReference.FindAction("UI/Interact").performed += OnSubmitInput;

        UIDialogueWindow.Instance.WasClickedOn += OnDialogueWindowClickedOn;
    }

    private void OnDestroy()
    {
        InputActionAsset inputReference = ControlManager.CurrentInput;
        inputReference.FindAction("UI/Interact").performed -= OnSubmitInput;

        UIDialogueWindow.Instance.WasClickedOn -= OnDialogueWindowClickedOn;
    }

    void OnDialogueWindowClickedOn()
    { 
        if(m_IsPlaying)
            PlayNext();
    }
    void OnSubmitInput(InputAction.CallbackContext callbackContext)
    {
        if(m_IsPlaying)
            PlayNext();
    }

    public void StartDialogue()
    {
        if (Phrases.Length > 0)
        {
            m_IsPlaying = true;
            m_NextIndex = 0;
            PlayNext();
            
            UIDialogueWindow.Instance.Show(this);
        }
    }

    public void ChooseNext(int choiceIndex)
    {
        choiceIndex = Mathf.Clamp(choiceIndex, 0, Phrases[m_CurrentIndex].NextTimelineIndexes.Length - 1);
        m_NextIndex = Phrases[m_CurrentIndex].NextTimelineIndexes[choiceIndex];
        
        PlayNext();
    }

    public bool HaveNextEntry()
    {
        return m_NextIndex != k_NoIndex;
    }
    
    void PlayNext()
    {
        if (!m_CanBeNexted)
        {
            if (m_CurrentPlayingDirector != null)
            {
                m_CurrentPlayingDirector.time = m_CurrentPlayingDirector.duration;
            }
            
            return;
        }
        

        if (m_NextIndex == k_NoIndex)
        {
            //we reached the end of the conversation, put the controller back into "normal" mode
            m_IsPlaying = false;

            UIDialogueWindow.Instance.Hide();
            if(CameraController)
                CameraController.SetConversationTargets(null, null);
            return;
        }
        
        if (m_NextIndex == k_NextIsChoice)
        {
            //we can't next we need to make a choice just return 
            return;
        }
        
        UIDialogueWindow.Instance.DisplayNextPrompt(false);

        DialoguePhrase nextPhrase = Phrases[m_NextIndex];
        nextPhrase.PlayableDirector.Play();
        if(CameraController)
            CameraController.SetConversationTargets(nextPhrase.SubtitleIdentifier, Character);

        //can't be nexted until the current timeline finish playing
        //TODO : need to allow to skip, but will need to handle finishing the timeline in one go so everything
        //is added to the dialogue window
        m_CanBeNexted = false;

        m_CurrentIndex = m_NextIndex;

        if (nextPhrase.NextTimelineIndexes.Length > 0)
        {
            if (nextPhrase.NextTimelineIndexes.Length == 1)
            {
                //a single next timeline mean this is just part of a linear dialogue so next to play is the next in line
                m_NextIndex = nextPhrase.NextTimelineIndexes[0];
            }
            else
            {
                m_NextIndex = k_NextIsChoice;
            }
        }
        else
        {
            m_NextIndex = k_NoIndex;
        }
    }
}
