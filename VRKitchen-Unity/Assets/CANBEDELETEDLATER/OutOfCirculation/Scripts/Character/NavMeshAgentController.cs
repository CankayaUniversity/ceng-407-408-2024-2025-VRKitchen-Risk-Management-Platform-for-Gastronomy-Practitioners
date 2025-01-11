using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class NavMeshAgentController : MonoBehaviour
{
    [Range(0f, 89f)]
    [Tooltip("The maximum angle between the direction the character is facing or input and the direction from the character to an interactable object for it to be registered by axis input.")]
    public float MaxAngleOfInteraction = 45f;
    [Tooltip("The maximum distance between the character and an interactable object for it to be registered by axis input.")]
    public float MaxDistanceOfInteraction = 2f;
    [Range(0f, 1f)]
    [Tooltip("When using axis input the desired interactable object is selected using a combination of the angle to the object and the distance to the object. Increasing this value will make the object being more in front of the character be more important when selecting.")]
    public float AngleAdditionalInteractionWeighting = 1f;
    [Tooltip("The distance from the character that is set by the nav mesh agent for axis input.")]
    public float AxisInputDestinationDistance = 0.2f;
    [Tooltip("The angle of a path from the character's forward direction that is allowed. The smaller this is the less the character will auto-path around corners.")]
    public float MaxPathAngleError = 10f;
    [Tooltip("The degrees per second the character turns towards its path's direction.")]
    public float OrientationInterpolationSpeed = 360f;
    [Tooltip("The distance of the character from an interaction location before the path is canceled and the character teleports there.")]
    public float TeleportToInteractableObjectDistance = 0.05f;
    [Space]
    [Tooltip("A reference to the nav mesh agent of the character.")]
    public NavMeshAgent NavMeshAgent;
    [Tooltip("The camera through which rays are cast when using pointer input.")]
    public Camera Cam;
    [Tooltip("The layers of objects that the raycast for pointer input should interact with.")]
    public LayerMask MouseRayLayerMask;
    [Header("Prefab Reference")]
    [Tooltip("A reference to the prefab of the position indicator for mouse movement.")]
    public GameObject MoveIndicatorPrefab;

    InputAction m_MouseClick;           // Various input actions so their state can be checked.
    InputAction m_MoveAction;
    InputAction m_InteractAction;
    float m_MinCosineOfInteraction;     // Caching the cosine of angles for faster evaluation.
    float m_MinCosineOfPathAngleError;
    Transform m_Transform;              // A reference to the character's transform.
    bool m_HasInteracted;               // Whether or not the character has interacted with the destination interactive object.
    bool m_AxisInputHeadedToInteractable;   // Whether or not the axis input is being used and an interactive object has been set as the destination.
    Vector3 m_InputInWorldSpace;        // The vector from the player character's position that axis input is pointing.
    BaseInteractiveObject m_DestinationInteractiveObject;   // The interactive object that the player's character is currently heading towards.
    BaseInteractiveObject m_DesiredInteractiveObject;       // The interactive object which will be set as the destination should there be an interaction input.
    BaseInteractiveObject[] m_AllInteractiveObjects;        // A collection of all the interactive objects in the scene.
    NavMeshPath m_PathBuffer;                           // A buffer to hold the current path on the nav mesh.
    Vector3[] m_PathCornersBuffer = new Vector3[10];    // A buffer to hold all the corners of the current path.
    GameObject m_MoveIndicator;         // The instantiated move indicator.

    const float k_RaycastDistance = 100f;       // The distance into the scene of the mouse's raycast on interaction input.
    const float k_SqrMinLookSpeed = 0.01f;      // The minimum required speed that causes the character's orientation to change.

    public Vector3 Velocity => NavMeshAgent.velocity;   // Used by the animation systems to match the animation to how the character is moving.
    public float AngularSpeed { get; private set; }     // Currently unused. If the animation systems are expanded to include turning animations, this will be required.

    void Reset()
    {
        NavMeshAgent = GetComponent<NavMeshAgent>();
        Cam = Camera.main;
        MouseRayLayerMask = LayerMask.GetMask("Floor", "Character", "CharacterOutline", "InteractiveObject", "InteractiveOutlined");
    }

    void Awake()
    {
        // Init the first scene it is created into.
        SceneInit();
        
        // Then register to be init at each scene load.
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            SceneInit();
        };
    }

    void Start()
    {
        // Cache and enable input.
        InputActionAsset inputReference = ControlManager.CurrentInput;
        inputReference.Enable();
        
        m_MouseClick = inputReference.FindAction("Gameplay/Pointer Interact");
        m_MoveAction = inputReference.FindAction("Gameplay/Move");
        m_InteractAction = inputReference.FindAction("Gameplay/Interact");
        m_MoveAction = inputReference.FindAction("Gameplay/Move");
        
        m_MouseClick.Enable();
        m_MoveAction.Enable();
        m_InteractAction.Enable();
        m_MoveAction.Enable();

        // Cache and initialize variables.
        m_MinCosineOfInteraction = Mathf.Cos(MaxAngleOfInteraction * Mathf.Deg2Rad);
        m_MinCosineOfPathAngleError = Mathf.Cos(MaxPathAngleError * Mathf.Deg2Rad);
        m_Transform = transform;
        m_PathBuffer = new NavMeshPath();
        
        // Rotation will be updated manually and so should be disabled on the Nav Mesh Agent.
        NavMeshAgent.updateRotation = false;

        // Create the indicator for mouse based input and ensure it is not destroyed between scenes.
        m_MoveIndicator = Instantiate(MoveIndicatorPrefab);
        m_MoveIndicator.SetActive(false);
        DontDestroyOnLoad(m_MoveIndicator);
    }

    public void SceneInit()
    {
        // Get references to per-scene objects.
        Cam = Camera.main;
        m_AllInteractiveObjects = FindObjectsOfType<BaseInteractiveObject>();
    }

    void Update()
    {
        // Cache whether or not there is a current destination to avoid repeated null checks.
        bool hasDestinationInteractiveObject = m_DestinationInteractiveObject != null;
        
        // If there is a desired interactive object but axis input isn't being used or the character is not headed to the interactive object then disable its highlight.
        if(m_DesiredInteractiveObject != null && !m_AxisInputHeadedToInteractable)
            m_DesiredInteractiveObject.Highlight(false);

        // Call the appropriate input control method.
        bool axisInput = ControlManager.CurrentControlType == ControlManager.ControlType.Gamepad || ControlManager.CurrentControlType == ControlManager.ControlType.Keyboard; 
        if (ControlManager.CurrentControlType == ControlManager.ControlType.Mouse)
            MouseInputControl(ref hasDestinationInteractiveObject);
        else if (axisInput)
            AxisInputControl(ref hasDestinationInteractiveObject);

        // If the character is headed to an interactive object, has not yet interacted and is within a short distance...
        if (hasDestinationInteractiveObject && !m_HasInteracted && !NavMeshAgent.pathPending && NavMeshAgent.remainingDistance <= TeleportToInteractableObjectDistance)
        {
            // ... teleport the character to the interaction location...
            m_Transform.position = m_DestinationInteractiveObject.interactionLocation.position;
            m_Transform.rotation = m_DestinationInteractiveObject.interactionLocation.rotation;
            
            // ... and interact with the object. Also clear cached variables for this object.
            m_DestinationInteractiveObject.Interact(this);
            m_DestinationInteractiveObject = null;
            m_AxisInputHeadedToInteractable = false;
        }

        // Cache variables for rotation calculation.
        Vector3 velocity = NavMeshAgent.velocity;
        Quaternion currentRotation = m_Transform.rotation;
        Quaternion desiredRotation = currentRotation;
        
        // If the character is moving then the desired rotation is in the direction they are moving.
        if (velocity.sqrMagnitude > k_SqrMinLookSpeed)
        {
            desiredRotation = Quaternion.LookRotation(velocity);
        }
        // Otherwise, if there is axis input, then the desired rotation is in the input direction.
        else if (m_InputInWorldSpace.sqrMagnitude > float.Epsilon)
        {
            desiredRotation = Quaternion.LookRotation(m_InputInWorldSpace);
        }

        // Calculate the angle change.
        float maxAngleChange = OrientationInterpolationSpeed * Time.deltaTime;
        float desiredAngleChange = Quaternion.Angle(currentRotation, desiredRotation);
        float actualAngleChange = Mathf.Min(maxAngleChange, desiredAngleChange);
        
        // Record the angular speed.
        AngularSpeed = actualAngleChange / Time.deltaTime;
        
        // Set the new rotation.
        m_Transform.rotation = Quaternion.RotateTowards(currentRotation, desiredRotation, maxAngleChange);
    }

    /// <summary>
    /// The governing method for all pointer (generally mouse) based input.
    /// </summary>
    void MouseInputControl(ref bool hasDestinationInteractiveObject)
    {
        // If the player has already clicked on an interactive object it will be blocking navigation input and so nothing more should be done.
        if(hasDestinationInteractiveObject && m_DestinationInteractiveObject.IsBlockingNavigationInput())
            return;

        // Cache input data.
        bool mouseClicked = m_MouseClick.ReadValue<float>() > InputSystem.settings.defaultButtonPressPoint;
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        
        // If the pointer isn't currently over a gameobject nothing else needs to be done.
        EventSystem current = EventSystem.current;
        if(current != null && EventSystem.current.IsPointerOverGameObject())
            return;
        
        // Cast a ray into the scene.
        Ray ray = Cam.ScreenPointToRay(mousePosition);
        Physics.Raycast(ray, out RaycastHit hit, k_RaycastDistance, MouseRayLayerMask.value,
            QueryTriggerInteraction.Collide);
            
        // Find the index of the interactable under the pointer. This will be -1 if there is none.        
        int interactableObjectIndex = FindDesiredInteractableIndexForMouseInput(hit);

        // If there is an interactive object beneath the pointer...
        if (interactableObjectIndex != -1)
        {
            // ... disable the move indicator...
            m_MoveIndicator.SetActive(false);
            
            // ... disable the highlight of the existing desired interactive object...
            if(m_DesiredInteractiveObject != null)
                m_DesiredInteractiveObject.Highlight(false);
            
            // ... and enable the highlight on the new desired interactive object. 
            m_DesiredInteractiveObject = m_AllInteractiveObjects[interactableObjectIndex];
            m_DesiredInteractiveObject.Highlight(true);

            // If the interactive object is clicked on set it as the destination.
            if (mouseClicked)
            {
                SetInteractionDestinationForMouseInput(out hasDestinationInteractiveObject);
            }
        }
        // If there is no interactive object beneath the pointer...
        else
        {
            // ... enable/disable the move indicator based on whether the mouse is above the nav mesh.
            if (NavMesh.SamplePosition(hit.point, out var navmeshHit, 1.0f, -1))
            {
                m_MoveIndicator.SetActive(true);
                m_MoveIndicator.transform.position = navmeshHit.position;
            }
            else
            {
                m_MoveIndicator.SetActive(false);
            }
            
            // If the mouse is clicked set the character's destination.
            if (mouseClicked)
                SetNonInteractionDestinationForMouseInput(out hasDestinationInteractiveObject, hit);
        }
    }

    /// <summary>
    /// Iterates through all the interactive objects and finds the one whose collider matches the raycast hit. 
    /// </summary>
    /// <param name="hit">The hit whose collider is matched to an interactive object.</param>
    /// <returns>The index of the interactive object.</returns>
    int FindDesiredInteractableIndexForMouseInput(RaycastHit hit)
    {
        int interactableObjectIndex = -1;
        for (int i = 0; i < m_AllInteractiveObjects.Length; i++)
        {
            if (m_AllInteractiveObjects[i].Col == hit.collider)
            {
                interactableObjectIndex = i;
                break;
            }
        }

        return interactableObjectIndex;
    }

    /// <summary>
    /// Sets the destination interactive object based on the current desired interactive object.
    /// </summary>
    void SetInteractionDestinationForMouseInput(out bool hasDestinationInteractiveObject)
    {
        m_DestinationInteractiveObject = m_DesiredInteractiveObject;
        hasDestinationInteractiveObject = true;
        NavMeshAgent.SetDestination(m_DestinationInteractiveObject.interactionLocation.position);
        m_HasInteracted = false;
    }

    /// <summary>
    /// Sets the destination to be the nearest point on the nav mesh to the point that was hit.
    /// </summary>
    void SetNonInteractionDestinationForMouseInput(out bool hasDestinationInteractiveObject, RaycastHit hit)
    {
        m_DestinationInteractiveObject = null;
        hasDestinationInteractiveObject = false;
        NavMeshAgent.SetDestination(hit.point);
        m_HasInteracted = false;
    }

    /// <summary>
    /// The governing method for all axis (generally keyboard or gamepad) based input.
    /// </summary>
    void AxisInputControl(ref bool hasDestinationInteractiveObject)
    {
        // If the player is already input to interact with an interactive object then it will be blocking input and nothing else should be done.
        if(hasDestinationInteractiveObject && m_DestinationInteractiveObject.IsBlockingNavigationInput())
            return;

        // Cache input data.
        Vector2 moveInput = m_MoveAction.ReadValue<Vector2>(); 
        bool hasAxisInput = moveInput.sqrMagnitude > float.Epsilon;
        bool hasInteractInput = m_InteractAction.ReadValue<float>() > InputSystem.settings.defaultButtonPressPoint;
        
        // Cache player position.
        Vector3 playerPosition = m_Transform.position;
        
        // Convert input as axes to a vector in world space.
        m_InputInWorldSpace = CalculateInputInWorldSpace(moveInput);

        // Cache the 2D position of the player.
        Vector3 flatPlayerPosition = playerPosition;
        flatPlayerPosition.y = 0f;

        // Find the index of the interactable based on input and player orientation. This will be -1 if there is none.
        int interactableObjectIndex = FindDesiredInteractableIndexForAxisInput(flatPlayerPosition, m_InputInWorldSpace, out float distanceToSelectedInteractable);

        // If an interactable object is found...
        if (interactableObjectIndex != -1)
        {
            // ... disable the highlight on the existing desired interactive object...
            if(m_DesiredInteractiveObject != null)
                m_DesiredInteractiveObject.Highlight(false);
            
            // ... and enable it on the new desired interactive object.
            m_DesiredInteractiveObject = m_AllInteractiveObjects[interactableObjectIndex];
            m_DesiredInteractiveObject.Highlight(true);

            // If the player is attempting to interact then set the destination as that interactive object.
            if (hasInteractInput)
                SetInteractionDestinationForAxisInput(out hasDestinationInteractiveObject, interactableObjectIndex, distanceToSelectedInteractable);
            // Otherwise if there is axis input set a destination on the nav mesh.
            else if (hasAxisInput)
                SetNonInteractionDestinationForAxisInput(out hasDestinationInteractiveObject, playerPosition, m_InputInWorldSpace);
        }
        // If there was no interactive object found but there is axis input then set a destination on the nav mesh.
        else if(hasAxisInput)
        {
            SetNonInteractionDestinationForAxisInput(out hasDestinationInteractiveObject, playerPosition, m_InputInWorldSpace);
        }
    }

    /// <summary>
    /// Sets the interactable object at the provided index as the destination for the character.
    /// </summary>
    void SetInteractionDestinationForAxisInput(out bool hasCurrentInteractiveObject, int interactableObjectIndex, float distanceToSelectedInteractable)
    {
        m_DestinationInteractiveObject = m_AllInteractiveObjects[interactableObjectIndex];
        hasCurrentInteractiveObject = true;

        // If the character is close enough to the interaction location then teleport them there and interact.
        if (distanceToSelectedInteractable < TeleportToInteractableObjectDistance)
        {
            m_Transform.position = m_DestinationInteractiveObject.interactionLocation.position;
            m_Transform.rotation = m_DestinationInteractiveObject.interactionLocation.rotation;
            m_DestinationInteractiveObject.Interact(this);
            m_DestinationInteractiveObject = null;
            hasCurrentInteractiveObject = false;
            m_AxisInputHeadedToInteractable = false;
        }
        // Otherwise set the NavMeshAgent's destination.
        else
        {
            NavMeshAgent.SetDestination(m_DestinationInteractiveObject.interactionLocation.position);
            m_AxisInputHeadedToInteractable = true;
        }
        
        m_HasInteracted = false;
    }

    /// <summary>
    /// Sets the destination to be the nearest point based on player position and input direction.
    /// </summary>
    void SetNonInteractionDestinationForAxisInput(out bool hasDestinationInteractiveObject, Vector3 playerPosition, Vector3 inputInWorldSpace)
    {
        hasDestinationInteractiveObject = false;
        
        // The destination is a small distance from the player in the direction of the input.
        Vector3 destination = playerPosition + inputInWorldSpace * AxisInputDestinationDistance;

        // Calculate a path to the destination.
        NavMesh.CalculatePath(playerPosition, destination, NavMesh.AllAreas, m_PathBuffer);

        // If a path is created...
        if (m_PathBuffer.status == NavMeshPathStatus.PathComplete)
        {
            // ... of more than 2 corners...
            int cornerCount = m_PathBuffer.GetCornersNonAlloc(m_PathCornersBuffer);
            if (cornerCount >= 2)
            {
                // ... the check the angle between the start of the path and the input's direction.
                Vector3 pathStart = m_PathCornersBuffer[1] - m_PathCornersBuffer[0];
                float cosine = Vector3.Dot(inputInWorldSpace, pathStart.normalized);
                
                // If that angle is too large (cosine is too small)...
                if (cosine < m_MinCosineOfPathAngleError)
                {
                    // ... then find the edge in the direction of the destination and set that as the destination.
                    // This is to prevent setting destinations on the other side of walls and other gaps in the nav mesh.
                    NavMesh.Raycast(playerPosition, destination, out NavMeshHit hit, NavMesh.AllAreas);
                    NavMeshAgent.SetDestination(hit.position);
                }
                // If the angle is sufficiently small then set the path.
                else
                {
                    NavMeshAgent.SetPath(m_PathBuffer);
                }
            }
        }
        else
        {
            // If the path is not complete then find the closest point on the nav mesh and set that as the destination.
            NavMesh.Raycast(playerPosition, destination, out NavMeshHit hit, NavMesh.AllAreas);
            if (hit.hit)
                NavMeshAgent.SetDestination(hit.position);
        }
        
        m_HasInteracted = false;
    }

    /// <summary>
    /// Uses the camera's orientation and axis input to create a vector for intended direction in world space.
    /// </summary>
    Vector3 CalculateInputInWorldSpace(Vector2 input)
    {
        Vector3 flatCameraForward = Cam.transform.forward;
        flatCameraForward.y = 0f;
        flatCameraForward.Normalize();

        Vector3 inputInLocalSpace = new Vector3(input.x, 0f, input.y);

        Vector3 inputInWorldSpace = Quaternion.LookRotation(flatCameraForward) * inputInLocalSpace;
        
        if(inputInWorldSpace.sqrMagnitude > float.Epsilon)
            inputInWorldSpace.Normalize();
        else
            inputInWorldSpace = Vector3.zero;
        
        return inputInWorldSpace;
    }

    /// <summary>
    /// Finds the interactable it is most likely the player wishes to interact with based on their input and the character's pose.
    /// </summary>
    /// <returns>The index of the most desirable interactive object</returns>
    int FindDesiredInteractableIndexForAxisInput(Vector3 playerPosition, Vector3 inputInWorldSpace, out float distanceToSelectedInteractable)
    {
        float inputBasedGreatestComparable = 0f;
        int inputBasedInteractableObjectIndex = -1;
        float inputBasedDistanceToSelectedInteractable = float.PositiveInfinity;
        float facingBasedGreatestComparable = 0f;
        int facingBasedInteractableObjectIndex = -1;
        float facingBasedDistanceToSelectedInteractable = float.PositiveInfinity;
        
        distanceToSelectedInteractable = float.PositiveInfinity;

        Vector3 characterForward = m_Transform.forward;

        // Iterate through all interactive objects in the scene.
        for (int i = 0; i < m_AllInteractiveObjects.Length; i++)
        {
            // If the interactive object is no enabled, skip it.
            if(!m_AllInteractiveObjects[i].gameObject.activeInHierarchy || !m_AllInteractiveObjects[i].enabled)
                continue;
            
            // Cache information about the character's position relative to the interactive object.
            Vector3 characterToInteractiveObject = m_AllInteractiveObjects[i].FlatPosition - playerPosition;
            float distanceToInteractable = characterToInteractiveObject.magnitude;
            float inverseDistance = 1f / distanceToInteractable;
            
            // Find the cosine of the angle between the input in world space and the vector between the player and interactive object.
            float cosineAngle = Vector3.Dot(characterToInteractiveObject, inputInWorldSpace) * inverseDistance;

            // If the angle is sufficiently small (cosine sufficiently large) and the distance is sufficiently small... 
            if (cosineAngle > m_MinCosineOfInteraction && distanceToInteractable < MaxDistanceOfInteraction)
            {
                // ... create a comparable representing the interactive object's suitability.
                float comparable = Mathf.Pow(cosineAngle, 1f + AngleAdditionalInteractionWeighting) * Mathf.Pow(inverseDistance, 2f - AngleAdditionalInteractionWeighting);

                // If this is the greatest value of comparable so far then cache it's information.
                if (comparable > inputBasedGreatestComparable)
                {
                    inputBasedGreatestComparable = comparable;
                    inputBasedInteractableObjectIndex = i;
                    inputBasedDistanceToSelectedInteractable = distanceToInteractable;
                }
            }

            // Repeat the above process for the same interactive object but use the character's forward vector in place of the input.
            cosineAngle = Vector3.Dot(characterToInteractiveObject, characterForward) * inverseDistance;

            if (cosineAngle > m_MinCosineOfInteraction && distanceToInteractable < MaxDistanceOfInteraction)
            {
                float comparable = Mathf.Pow(cosineAngle, 1f + AngleAdditionalInteractionWeighting) * Mathf.Pow(inverseDistance, 2f - AngleAdditionalInteractionWeighting);

                if (comparable > facingBasedGreatestComparable)
                {
                    facingBasedGreatestComparable = comparable;
                    facingBasedInteractableObjectIndex = i;
                    facingBasedDistanceToSelectedInteractable = distanceToInteractable;
                }
            }
        }

        // If a suitable interactive object was not found using input then return the facing based interactive object's details.
        if (inputBasedInteractableObjectIndex == -1)
        {
            distanceToSelectedInteractable = facingBasedDistanceToSelectedInteractable;
            return facingBasedInteractableObjectIndex;
        }

        // Otherwise return the input based object's details.
        distanceToSelectedInteractable = inputBasedDistanceToSelectedInteractable;
        return inputBasedInteractableObjectIndex;
    }
}