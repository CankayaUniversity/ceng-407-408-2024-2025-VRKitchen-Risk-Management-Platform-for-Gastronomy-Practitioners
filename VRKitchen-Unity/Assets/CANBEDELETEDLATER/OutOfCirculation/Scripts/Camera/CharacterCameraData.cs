using System;
using UnityEngine;

[Serializable]
public class CharacterCameraData
{
    [Tooltip("The Subtitle Identifier that is used when this particular character is speaking.")]
    public SubtitleIdentifier SubtitleIdentifier;
    [Tooltip("The Transform that the camera should look at when the referenced Subtitle Identifier is being used in conversation.")]
    public Transform CameraTarget;
    [Tooltip("How the camera should be positioned relative to the character when they are speaking.")]
    public ConversationCameraDistanceSettings SpeakerCameraSettings = ConversationCameraDistanceSettings.Default;
}