using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("A reference to the child transform whose pose is used for the camera when dynamic cameras are turned off.")]
    public Transform StaticCameraPose;
    [Tooltip("A reference to the mixing camera that allows manual blending of weights between the various virtual cameras.")]
    public CinemachineMixingCamera CinemachineMixingCamera;
    [Tooltip("A reference to the Cinemachine Brain for the camera to be controlled.")]
    public CinemachineBrain CinemachineBrain;
    [Tooltip("The SubtitleIdentifier for the player's character. Used to differentiate between characters in conversation.")]
    public SubtitleIdentifier Player;
    [Range(1f, 5f)]
    [Tooltip("The exponent part of the virtual camera's weight calculation. Increasing this will make the camera movement more snappy, decreasing it will make the movement smoother.")]
    public float DynamicCameraWeightScaling = 1.8f;
    [Tooltip("Pairings of waypoints in the scene and virtual cameras. When the player character is near a waypoint it will increase the weight given to its virtual camera.")]
    public VirtualCameraMapping[] CameraMappings;
    [Tooltip("Pairings of Subtitle Identifiers and the Transform that should be aimed at when it is being used in conversation. Note that the player's character is spawned and so is expected to have a null transform.")]
    public CharacterCameraData[] CharacterMappings;
    
    Transform m_Player;                 // A reference to the player's transform so its position can be checked.
    Transform m_CameraTransform;        // A reference to the camera's transform so it can be posed.
    Transform m_Speaker;                // The transform identified as the current speaker during conversation.
    Transform m_Listener;               // The transform identified as the current listener during the conversation.
    ConversationCameraDistanceSettings m_CurrentDistanceSettings;   // The settings to position the camera relative to the listener.
    bool m_IsInConversation;            // Whether or not there is currently a conversation happening.

    void Reset()
    {
        // Try to find all the default references as per the prefab.
        StaticCameraPose = transform.Find("StaticCameraPose");
        CinemachineMixingCamera = GetComponent<CinemachineMixingCamera>();
        CameraMappings = new VirtualCameraMapping[2];
        CameraMappings[0] = new VirtualCameraMapping
        {
            CharacterWaypoint = transform.Find("Waypoint0"),
            Vcam = transform.Find("CM vcam0").GetComponent<CinemachineVirtualCameraBase>()
        };
        CameraMappings[1] = new VirtualCameraMapping
        {
            CharacterWaypoint = transform.Find("Waypoint1"),
            Vcam = transform.Find("CM vcam1").GetComponent<CinemachineVirtualCameraBase>()
        };
    }

    /// <summary>
    /// Sets the speaker and listener transforms based on the Subtitle Identifiers currently being used.
    /// </summary>
    /// <param name="speaker">The Subtitle Identifier for whichever character is currently speaking.</param>
    /// <param name="nonPlayerCharacter">The Subtitle Identifier for whichever character the player's character is in conversation with.</param>
    public void SetConversationTargets(SubtitleIdentifier speaker, SubtitleIdentifier nonPlayerCharacter)
    {
        // If both Subtitle Identifiers are null then the conversation has ended.
        if (speaker == null && nonPlayerCharacter == null)
        {
            m_IsInConversation = false;
            return;
        }

        // If the speaker is the same as the character the player's character is in conversation with...
        if (speaker == nonPlayerCharacter)
        {
            // ... then the listener must be the player...
            m_Listener = m_Player;

            // ... and the speaker can be found by iterating through the mappings.
            for (int i = 0; i < CharacterMappings.Length; i++)
            {
                if (CharacterMappings[i].SubtitleIdentifier == speaker)
                {
                    m_Speaker = CharacterMappings[i].CameraTarget;
                    m_CurrentDistanceSettings = CharacterMappings[i].SpeakerCameraSettings;
                    break;
                }
            }
        }
        else
        {
            // Otherwise the speaker must be the player...
            m_Speaker = m_Player;
            
            // ... and we can iterate through the mappings to find the non-player character which must be the listen,
            // as well as find the player's distance settings.
            for (int i = 0; i < CharacterMappings.Length; i++)
            {
                if (CharacterMappings[i].SubtitleIdentifier == nonPlayerCharacter)
                {
                    m_Listener = CharacterMappings[i].CameraTarget;
                }

                if (CharacterMappings[i].SubtitleIdentifier == Player)
                {
                    m_CurrentDistanceSettings = CharacterMappings[i].SpeakerCameraSettings;
                }
            }
        }
        
        m_IsInConversation = true;
    }
    
    void OnEnable()
    {
        // Subscribe to the character spawn event on all the spawn points.
        SpawnPoint[] spawnPoints = FindObjectsOfType<SpawnPoint>();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spawnPoints[i].OnPlayerCharacterSpawn += WhenPlayerSpawns;
        }
    }

    void OnDisable()
    {
        // Unsubscribe from the spawn event on all the spawn points.
        SpawnPoint[] spawnPoints = FindObjectsOfType<SpawnPoint>();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spawnPoints[i].OnPlayerCharacterSpawn -= WhenPlayerSpawns;
        }
    }

    /// <summary>
    /// Called when the player's character spawns. Used to cache a reference to the player character's transform and set LookAt targets where necessary.
    /// </summary>
    /// <param name="spawnPoint">The SpawnPoint at which the player's character spawned.</param>
    /// <param name="navMeshAgentController">The script responsible for moving the player's character.</param>
    void WhenPlayerSpawns(SpawnPoint spawnPoint, NavMeshAgentController navMeshAgentController)
    {
        m_Player = navMeshAgentController.transform;

        for (int i = 0; i < CameraMappings.Length; i++)
        {
            if (CameraMappings[i].LookAtPlayerCharacter)
                CameraMappings[i].Vcam.LookAt = m_Player;
        }
    }

    void Start()
    {
        // Sort the virtual camera mappings so they are in the same order as they are on the cinemachine mixing camera.
        // This is to make iterating through the virtual cameras easier when setting their weights.
        List<VirtualCameraMapping> mappingList = new List<VirtualCameraMapping>(CameraMappings);

        for (int i = 0; i < CinemachineMixingCamera.ChildCameras.Length; i++)
        {
            CinemachineVirtualCameraBase vcam = CinemachineMixingCamera.ChildCameras[i];
            
            if(mappingList.Count > i && vcam == mappingList[i].Vcam)
                continue;

            bool wasMappingMoved = false;
            for (int j = i + 1; j < mappingList.Count; j++)
            {
                VirtualCameraMapping virtualCameraMapping = mappingList[j];
                
                if (vcam == virtualCameraMapping.Vcam)
                {
                    mappingList.RemoveAt(j);
                    mappingList.Insert(i, virtualCameraMapping);
                    wasMappingMoved = true;
                }
            }

            if (!wasMappingMoved)
            {
                mappingList.Insert(i, new VirtualCameraMapping());
            }
        }

        CameraMappings = mappingList.ToArray();

        m_CameraTransform = CinemachineBrain.transform;
    }

    void Update()
    {
        // If the dynamic cameras are off, disable cinemachine and set the camera's pose directly.
        if(!SaveSystem.CurrentSettings.UseDynamicCameras)
        {
            CinemachineBrain.enabled = false;
            m_CameraTransform.position = StaticCameraPose.position;
            m_CameraTransform.rotation = StaticCameraPose.rotation;
            
            return;
        }

        // If there is a conversation currently happening...
        if (m_IsInConversation)
        {
            // Create a flattened unit vector from the speaker's position to the listeners position. 
            Vector3 speakerToListener = m_Listener.position - m_Speaker.position;
            speakerToListener.y = 0f;
            speakerToListener.Normalize();
            
            // Create a perpendicular unit vector between the speaker and listener.
            Vector3 conversationIntersection = Vector3.Cross(speakerToListener, Vector3.up).normalized;
            
            // If this perpendicular vector is in the opposite direction to the static camera's forward vector then flip its direction.
            Vector3 defaultForward = StaticCameraPose.forward;
            defaultForward.y = 0f;
            if (Vector3.Dot(conversationIntersection, defaultForward) > 0f)
                conversationIntersection *= -1f;

            // Disable cinemachine.
            CinemachineBrain.enabled = false;
            
            // Start the camera's position calculation at the listener.
            Vector3 cameraPosition = m_Listener.position;
            
            // Move the position laterally away from both speaker and listener.
            cameraPosition += conversationIntersection * m_CurrentDistanceSettings.LateralDistance;
            
            // Move the position up.
            cameraPosition += Vector3.up * m_CurrentDistanceSettings.Height;
            
            // Move the position away from the speaker, beyond the listener.
            cameraPosition += speakerToListener * m_CurrentDistanceSettings.OverShoulderDistance;
            
            // Set the camera's pose based on these calculations.
            m_CameraTransform.position = cameraPosition;
            m_CameraTransform.LookAt(m_Speaker);
            
            return;
        }

        // If using dynamic cameras and not in conversation then enable cinemachine.
        CinemachineBrain.enabled = true;
        
        // Iterate through all the camera mappings to find the longest distance from a waypoint to the player character's position and the total distance.
        float longestDistance = 0f;
        float totalDistance = 0f;
        for (int i = 0; i < CameraMappings.Length; i++)
        {
            if(!CameraMappings[i].IsValid())
                continue;
            
            float distance = CameraMappings[i].GetDistance(m_Player.position);

            totalDistance += distance;
            
            if (distance > longestDistance)
                longestDistance = distance;
        }

        // If there are only 2 virtual cameras then a slightly different algorithm is used to calculate the weight.
        float measuredDistance = CameraMappings.Length == 2 ? totalDistance : longestDistance;

        for (int i = 0; i < CameraMappings.Length; i++)
        {
            var weight = Mathf.Pow((measuredDistance - CameraMappings[i].DistanceToPlayer) / measuredDistance, DynamicCameraWeightScaling);
            CinemachineMixingCamera.SetWeight(i, weight);
        }
    }
}
