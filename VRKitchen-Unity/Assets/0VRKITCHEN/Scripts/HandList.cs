using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class HandTextDisplay : MonoBehaviour
{
    public GameObject textPlane; // Plane displaying the text
    public GameObject rightControllerVisual; // Visual representation of the right controller
    public ActionBasedController rightController; // Reference to the ActionBasedController

    private bool isVisible = false; // Initial state of the text display
    private TextMeshPro textMeshPro; // Reference to TextMeshPro component on the textPlane
    private MeshRenderer textRenderer; // To toggle visibility instead of deactivating

    void Start()
    {
        Debug.Log("HandTextDisplay script started");

        if (rightController == null)
        {
            Debug.LogError("Right Controller (Action-based) is not assigned!");
            return;
        }

        textMeshPro = textPlane.GetComponentInChildren<TextMeshPro>(); // Get TextMeshPro inside the plane
        textRenderer = textPlane.GetComponent<MeshRenderer>();

        if (textMeshPro == null)
        {
            Debug.LogError("TextMeshPro component is missing inside the textPlane!");
        }

        // Set TextMeshPro Background Color
        textMeshPro.enableWordWrapping = true;
        textMeshPro.overflowMode = TextOverflowModes.Overflow;
        textMeshPro.fontSizeMin = 10;
        textMeshPro.fontSizeMax = 50;

        // Set text background color (RGBA format)
        textMeshPro.fontMaterial.SetColor("_FaceColor", new Color(1, 1, 1, 1)); // White text
        textMeshPro.fontMaterial.SetColor("_OutlineColor", new Color(0, 0, 0, 1)); // Black outline
        textMeshPro.fontMaterial.SetFloat("_OutlineWidth", 0.2f); // Outline thickness
        textMeshPro.enableAutoSizing = true;

        SetVisibility(isVisible);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            isVisible = !isVisible;
            SetVisibility(isVisible);
        }

        if (rightControllerVisual != null)
        {
            Transform rightHandTransform = rightControllerVisual.transform;

            if (rightHandTransform != null)
            {
                textPlane.transform.position = rightHandTransform.position + rightHandTransform.up * 0.15f;

                Vector3 cameraPosition = Camera.main.transform.position;
                Vector3 directionToCamera = (cameraPosition - textPlane.transform.position).normalized;

                textPlane.transform.forward = -directionToCamera; // Face user
            }
        }
    }

    void SetVisibility(bool visible)
    {
        if (textRenderer != null)
        {
            textRenderer.enabled = false; // Always disable the plane's mesh renderer
        }

        if (textMeshPro != null)
        {
            textMeshPro.enabled = visible;
        }
    }

    public void DisplayResponseText(string responseText)
    {
        if (textMeshPro != null)
        {
            textMeshPro.text = responseText;
            textMeshPro.ForceMeshUpdate();
        }
    }
}
