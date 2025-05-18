using UnityEngine;

public class SpongeWetnessController : MonoBehaviour
{
    public float wetDuration = 5f;
    [HideInInspector] public bool isWet;

    private float wetTimer;
    private bool hasSentQuery = false;

    public UnityToAPI toAPI; // Used to send query to RAG system

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WaterSource"))
        {
            isWet = true;
            wetTimer = wetDuration;
            hasSentQuery = false;

            Debug.Log("Sponge is now wet.");

            // Trigger query to RAG for next contamination cleanup step
            if (toAPI != null && !hasSentQuery)
            {
                toAPI.queryText = "The sponge is wet now. What is the next step to handle cross contamination? Please provide only the next in-game step.";
                toAPI.SubmitQuery();
                hasSentQuery = true;
            }
        }
    }

    private void Update()
    {
        if (!isWet) return;

        wetTimer -= Time.deltaTime;
        if (wetTimer <= 0f)
        {
            isWet = false;
            hasSentQuery = false;
            Debug.Log("Sponge has dried.");
        }
    }
}
