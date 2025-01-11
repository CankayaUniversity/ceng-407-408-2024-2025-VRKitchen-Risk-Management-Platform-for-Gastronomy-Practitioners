using System;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [Tooltip("A reference to the animator component to be controlled.")]
    public Animator Animator;
    [Tooltip("A reference to the script controlling the movement of the character.")]
    public NavMeshAgentController NavMeshAgentController;

    float m_InverseWalkingAnimationSpeed;   // Used to speed up calculations.

    static readonly int k_HashSpeedPara = Animator.StringToHash("Speed");   // Used to set the speed parameter more efficiently.

    const float k_WalkingAnimationSpeed = 1f;   // An estimate for the speed the character is walking.

    void Reset()
    {
        // Try to set the default references as per the prefab.
        Animator = GetComponentInChildren<Animator>();
        NavMeshAgentController = GetComponent<NavMeshAgentController>();
    }

    void Start()
    {
        // Cache the inverse so future calculations can be faster.
        m_InverseWalkingAnimationSpeed = 1f / k_WalkingAnimationSpeed;
    }

    void Update()
    {
        // Find and set the speed parameter on the animator controller.
        float speed = NavMeshAgentController.Velocity.magnitude;
        Animator.SetFloat(k_HashSpeedPara, speed);
        
        // If the character is moving faster than the speed of the animation, speed up the animation to match.
        Animator.speed = speed > k_WalkingAnimationSpeed ? speed * m_InverseWalkingAnimationSpeed : 1f;
    }
}
