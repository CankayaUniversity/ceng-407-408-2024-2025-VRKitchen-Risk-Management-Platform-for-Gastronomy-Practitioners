using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class HandTextDisplay : MonoBehaviour
{
    public GameObject textPlane; // Plane displaying the text
    public GameObject rightControllerVisual; // Visual representation of the right controller
    private bool isVisible = true; // Make sure the text is always visible

    public ActionBasedController rightController; // Reference to the ActionBasedController
    private TextMeshPro textMeshPro; // Reference to TextMeshPro component on the textPlane

    void Start()
    {
        Debug.Log("HandTextDisplay script started");

        // Check if the right controller is assigned
        if (rightController == null)
        {
            Debug.LogError("Right Controller (Action-based) is not assigned!");
            return;
        }

        // Ensure the textPlane has a TextMeshPro component
        textMeshPro = textPlane.GetComponent<TextMeshPro>();
        if (textMeshPro == null)
        {
            Debug.LogError("TextMeshPro component is missing on the textPlane!");
        }

        textPlane.SetActive(true); // Ensure text plane is active

        // Enable wrapping overflow and set auto-sizing options
        textMeshPro.enableWordWrapping = true;
        textMeshPro.overflowMode = TextOverflowModes.Overflow; // Text overflow behavior

        // Optionally adjust max/min font size for better control in VR
        textMeshPro.fontSizeMin = 10;  // Minimum font size
        textMeshPro.fontSizeMax = 50;  // Maximum font size
    }

    void Update()
    {
        // Adjust the position of the text above the hand
        if (rightControllerVisual != null)
        {
            Transform rightHandTransform = rightControllerVisual.transform;

            if (rightHandTransform != null)
            {
                // Position the text plane above the right hand with a slight offset
                textPlane.transform.position = rightHandTransform.position + rightHandTransform.up * 0.15f; // Adjust vertical offset
                textPlane.transform.rotation = rightHandTransform.rotation; // Align text plane rotation with hand
            }
        }
    }

    public void DisplayResponseText(string responseText)
    {
        // Update the text plane with the API response
        if (textMeshPro != null)
        {
            textMeshPro.text = responseText;

            // Optionally adjust the auto-size to fit the text more neatly
            textMeshPro.ForceMeshUpdate(); // Ensure text mesh updates immediately
        }
    }
}
