using UnityEngine;

public class OvenInteraction : MonoBehaviour
{
    [Header("Button Reference")]
    public Transform buttonTransform; // Assign the button (child cube) in the Inspector

    [Header("API Reference")]
    public UnityToAPI apiScript; // Reference to the UnityToAPI script

    private bool isOvenOpen = false; // Track oven state

    private void Start()
    {
        // Validate references
        if (buttonTransform == null)
        {
            Debug.LogError("Button Transform is not assigned in the Inspector!");
        }

        if (apiScript == null)
        {
            Debug.LogError("UnityToAPI script is not assigned in the Inspector!");
        }
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

        // Only send a query if the oven is opened
        if (isOvenOpen)
        {
            string query = "Stove opened, what's the next step in the game?";

            // Send query to the API
            if (apiScript != null)
            {
                apiScript.queryText = query; // Set the query text
                apiScript.SubmitQuery(); // Submit the query
            }
            else
            {
                Debug.LogError("UnityToAPI script is not assigned!");
            }

            Debug.Log($"Oven state changed: {query}");
        }
        else
        {
            Debug.Log("Oven closed. No query sent.");
        }
    }
}