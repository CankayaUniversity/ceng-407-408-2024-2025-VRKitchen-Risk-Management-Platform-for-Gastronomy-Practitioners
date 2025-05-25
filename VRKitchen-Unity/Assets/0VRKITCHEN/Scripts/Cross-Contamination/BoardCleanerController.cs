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
                if (crossController != null)
                {
                                    Debug.Log("nulll.");

                    crossController.ResetContamination();
                }

                scrubTimer = 0f;
                sponge.isWet = false;
                Debug.Log("Board cleaned with sponge.");

                if (!hasSentQuery && toAPI != null)
                {
                    toAPI.queryText = RagCommands.CleanTheBoard; // bura
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
