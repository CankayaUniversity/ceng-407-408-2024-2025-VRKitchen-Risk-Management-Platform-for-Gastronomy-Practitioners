using UnityEngine;

public class BoardCleanerController : MonoBehaviour
{
    public float scrubDuration = 2f;
    public SimpleCross crossController;
    public VisualFeedbackController visualFeedback;
    public UnityToAPI toAPI;

    private float scrubTimer;
    private bool hasSentQuery = false;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Sponge")) return;

        var sponge = other.GetComponent<SpongeWetnessController>();

        if (sponge != null && sponge.isWet)
        {
            scrubTimer += Time.deltaTime;

            if (scrubTimer >= scrubDuration)
            {
                if (crossController != null && crossController.isContamination)
                {
                    crossController.ResetContamination();
                    visualFeedback?.HideExclamation();
                }

                scrubTimer = 0f;
                sponge.isWet = false;
                Debug.Log("Board cleaned with sponge.");

                if (!hasSentQuery && toAPI != null)
                {
                    toAPI.queryText = "The contaminated board has been cleaned. What is the next step in the game to handle cross contamination? Provide only the next in-game step.";
                    toAPI.SubmitQuery();
                    hasSentQuery = true;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Sponge"))
        {
            scrubTimer = 0f;
            hasSentQuery = false;
        }
    }
}
