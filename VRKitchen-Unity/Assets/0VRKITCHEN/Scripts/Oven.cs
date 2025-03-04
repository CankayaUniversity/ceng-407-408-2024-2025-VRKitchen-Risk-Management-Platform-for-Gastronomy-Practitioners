using UnityEngine;

public class OvenInteraction : MonoBehaviour
{
    [Header("Button Reference")]
    public Transform buttonTransform; // Assign the button (child cube) in the Inspector

    private bool isOvenOpen = false; // Track oven state
    private UnityToAPI apiScript; // Reference to API script

    private void Start()
    {
        //// Find UnityToAPI script in the scene
        //apiScript = FindObjectOfType<UnityToAPI>();
        //if (apiScript == null)
        //{
        //    Debug.LogError("UnityToAPI script not found in the scene!");
        //}

        //if (buttonTransform == null)
        //{
        //    Debug.LogError("Button Transform is not assigned in the Inspector!");
        //}
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the player's hand touched the button
        if (collision.gameObject.CompareTag("PlayerHand") && buttonTransform != null)
        {
            float distance = Vector3.Distance(collision.transform.position, buttonTransform.position);

            // Ensure the hand is close enough to the button
            if (distance < 0.1f)
            {
                ToggleOvenState();
            }
        }
    }

    private void ToggleOvenState()
    {
        isOvenOpen = !isOvenOpen; // Toggle state

        string query = isOvenOpen ? "oven is open" : "oven is closed";

        //// Send query to the API
        //if (apiScript != null)
        //{
        //    apiScript.queryText = query;
        //    apiScript.SubmitQuery();
        //}

        Debug.Log($"Oven state changed: {query}");
    }
}
