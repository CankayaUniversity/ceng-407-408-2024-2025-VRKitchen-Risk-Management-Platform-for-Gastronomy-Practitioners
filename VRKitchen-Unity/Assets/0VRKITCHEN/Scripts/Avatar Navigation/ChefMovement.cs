using UnityEngine;
using UnityEngine.AI;

public class ChefMovement : MonoBehaviour
{
    public Transform hazardTarget;
    public Transform returnPoint;
    public Animator animator;

    public Transform firePoint; // new fire destination point

    private NavMeshAgent agent;
    private bool goingToHazard = false;
    private bool returning = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent missing on WomanChef!");
        }
        else if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("WomanChef not on NavMesh at Start.");
        }
    }

    private void Update()
    {
        if (!IsAgentReady()) return;

        float velocity = agent.velocity.magnitude;
        animator.SetBool("isWalking", velocity > 0.1f);

        // Detect arrival at hazard or return point
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (goingToHazard)
            {
                goingToHazard = false;
                Debug.Log("Chef reached the hazard.");
            }
            else if (returning)
            {
                returning = false;
                Debug.Log("Chef returned to idle point.");
            }

            animator.SetBool("isWalking", false);
        }
    }

    public void GoToHazard()
    {
        if (IsAgentReady() && hazardTarget != null)
        {
            agent.SetDestination(hazardTarget.position);
            animator.SetBool("isWalking", true);
            goingToHazard = true;
            returning = false;
        }
    }

    public void GoToFire()
{
    if (IsAgentReady() && firePoint != null)
    {
        agent.SetDestination(firePoint.position);
        animator.SetBool("isWalking", true);
    }
}


    public void ReturnToStart()
    {
        if (IsAgentReady() && returnPoint != null)
        {
            agent.SetDestination(returnPoint.position);
            animator.SetBool("isWalking", true);
            returning = true;
            goingToHazard = false;
        }
    }

    public void GoToLocation(Vector3 targetPosition)
{
    if (IsAgentReady())
    {
        agent.SetDestination(targetPosition);
        animator.SetBool("isWalking", true);
    }
}


    private bool IsAgentReady()
    {
        return agent != null && agent.isOnNavMesh && agent.enabled;
    }
}
