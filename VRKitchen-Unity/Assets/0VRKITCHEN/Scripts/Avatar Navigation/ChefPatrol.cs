using UnityEngine;
using UnityEngine.AI;

public class ChefPatrol : MonoBehaviour
{
    public Transform[] patrolPoints;
    private int currentPoint = 0;
    private NavMeshAgent agent;
    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[0].position);
    }

    void Update()
    {
        // Move to next point when close enough
        if (!agent.pathPending && agent.remainingDistance < 0.3f)
        {
            GoToNextPoint();
        }

        // Drive walk animation
        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    void GoToNextPoint()
    {
        if (patrolPoints.Length == 0) return;

        currentPoint = (currentPoint + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[currentPoint].position);
    }

    // 🔊 Talking animation control
    public void SetTalkingState(bool isTalking)
    {
        if (animator != null)
        {
            animator.SetBool("Talking", isTalking);
        }
    }
}
