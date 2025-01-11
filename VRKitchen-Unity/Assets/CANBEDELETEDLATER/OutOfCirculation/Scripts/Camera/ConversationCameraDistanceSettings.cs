using System;
using UnityEngine;

[Serializable]
public struct ConversationCameraDistanceSettings
{
    [Tooltip("The height above the listener's position.")]
    public float Height;
    [Tooltip("The perpendicular distance from the speaker and listener.")]
    public float LateralDistance;
    [Tooltip("The distance beyond the listener from the speaker. Use a negative value if the camera should be in front of the listener.")]
    public float OverShoulderDistance;

    public static ConversationCameraDistanceSettings Default => new()
    {
        Height = 1f, LateralDistance = 2f, OverShoulderDistance = 1f
    };
}