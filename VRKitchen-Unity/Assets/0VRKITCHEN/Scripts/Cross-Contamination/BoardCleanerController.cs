using UnityEngine;

public class BoardCleanerController : MonoBehaviour
{
    public float scrubDuration = 2f;
    public SimpleCross crossController;
    public VisualFeedbackController visualFeedback;
    public UnityToAPI toAPI;

    private float scrubTimer;
    private bool hasSentQuery = false;

    private void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Sponge")) return;

        collision.rigidbody?.WakeUp();

        var sponge = collision.gameObject.GetComponent<SpongeWetnessController>();

        //Debug.Log($"Scrubbing... Time: {scrubTimer:F2}, Sponge Wet: {sponge.isWet}");

        if (sponge != null && sponge.isWet)
        {
            scrubTimer += Time.deltaTime;

            if (scrubTimer >= scrubDuration)
            {
                if (crossController != null && crossController.isContamination)
                {
                    crossController.ResetContamination();

                    if (visualFeedback != null)
                        visualFeedback.HideExclamation(); // Hide exclamation after successful scrub
                }

                scrubTimer = 0f;
                sponge.isWet = false;
                Debug.Log("Board cleaned with sponge.");

                if ( toAPI != null)
                {
                    toAPI.queryText = "The contaminated board has been cleaned. What is the next step in the game to handle cross contamination? Provide only the next in-game step.";
                    toAPI.SubmitQuery();
                    Debug.Log(toAPI.queryText);
                    hasSentQuery = true;
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Sponge"))
        {
            scrubTimer = 0f;
            hasSentQuery = false;
        }
    }
}
