using System;
using Cinemachine;
using UnityEngine;

[Serializable]
public struct VirtualCameraMapping
{
    [Tooltip("A waypoint in the scene. This should be somewhere that the player's character can walk to. When the player's character is near, more weight will be given to the virtual camera.")]
    public Transform CharacterWaypoint;
    [Tooltip("The virtual camera that will be given more weight when the player's character is near the waypoint.")]
    public CinemachineVirtualCameraBase Vcam;
    [Tooltip("When enabled, the Aim settings for the virtual camera can be used. Note this may not work as intended on older versions of Cinemachine.")]
    public bool LookAtPlayerCharacter;
    [HideInInspector]
    public float DistanceToPlayer;

    /// <summary>
    /// Whether or not this mapping has both a waypoint and virtual camera reference.
    /// </summary>
    /// <returns>Returns true when the character waypoint and virtual camera are both non-null.</returns>
    public bool IsValid()
    {
        return CharacterWaypoint && Vcam;
    }

    /// <summary>
    /// Gets and stores the distance from the given position to the waypoint.
    /// </summary>
    /// <param name="playerPosition">The position of the player's character.</param>
    /// <returns>The distance from the given position to the waypoint.</returns>
    public float GetDistance(Vector3 playerPosition)
    {
        float distance = Vector3.Distance(playerPosition, CharacterWaypoint.position);
        DistanceToPlayer = distance;
        return distance;
    }
}