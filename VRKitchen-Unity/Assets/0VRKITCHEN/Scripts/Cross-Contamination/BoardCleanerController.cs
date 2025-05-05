using UnityEngine;

public class BoardCleanerController : MonoBehaviour
{
    public float scrubDuration = 2f;
    public SimpleCross crossController;
    public VisualFeedbackController visualFeedback;

    private float scrubTimer;

    private void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Sponge")) return;

        var sponge = collision.gameObject.GetComponent<SpongeWetnessController>();
        if (sponge != null && sponge.isWet)
        {
            scrubTimer += Time.deltaTime;

            if (scrubTimer >= scrubDuration)
            {
                crossController?.ResetContamination();
                scrubTimer = 0f;
                sponge.isWet = false;
                Debug.Log("Board cleaned with sponge.");
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Sponge"))
            scrubTimer = 0f;
    }
}
