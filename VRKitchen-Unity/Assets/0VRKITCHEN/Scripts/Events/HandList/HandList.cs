using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class HandTextDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject clipBoard; // Should be ClipBoardVisual_2
    [SerializeField] private GameObject textPlane; // Should be HandListText
    [SerializeField] private GameObject rightControllerVisual;
    [SerializeField] private ActionBasedController rightController;

    private bool isZoomed = false;


    // Zooming Option

    private bool isVisible = false;
    private TMP_Text textMeshPro; // Use TMP_Text for compatibility with both TMP types

    void Start()
    {
        Debug.Log("HandTextDisplay script started");

        if (rightController == null)
        {
            Debug.LogError("Right Controller is not assigned!");
            return;
        }

        // Try getting TMP component directly or from children
        textMeshPro = textPlane.GetComponent<TMP_Text>();
        if (textMeshPro == null)
        {
            textMeshPro = textPlane.GetComponentInChildren<TMP_Text>();
        }

        if (textMeshPro == null)
        {
            Debug.LogError("No TextMeshPro or TMP_Text component found on or under textPlane!");
            return;
        }

        // Text settings
        textMeshPro.enableAutoSizing = true;
        textMeshPro.enableWordWrapping = true;
        textMeshPro.overflowMode = TextOverflowModes.Overflow;
        textMeshPro.fontSizeMin = 10;
        textMeshPro.fontSizeMax = 50;

        if (textMeshPro.fontMaterial.HasProperty("_FaceColor"))
            textMeshPro.fontMaterial.SetColor("_FaceColor", Color.white);

        if (textMeshPro.fontMaterial.HasProperty("_OutlineColor"))
            textMeshPro.fontMaterial.SetColor("_OutlineColor", Color.black);

        if (textMeshPro.fontMaterial.HasProperty("_OutlineWidth"))
            textMeshPro.fontMaterial.SetFloat("_OutlineWidth", 0.2f);

        // Initially hidden
        clipBoard.SetActive(isVisible);
    }

    void Update()
    {

        // Toggle clipboard visibility with
        if (Input.GetKeyDown(KeyCode.L) || rightController.activateAction.action.triggered)
        {
            isVisible = !isVisible;
            clipBoard.SetActive(isVisible);
        }

        if (!isVisible || rightControllerVisual == null)
            return;

        // Follow hand + face camera
        Transform hand = rightControllerVisual.transform;

        // Zoom offset
        float zoomOffset = isZoomed ? -0.4f : 0f;

        // Position slightly above hand and optionally closer to camera when zoomed
        Vector3 offset = hand.up * 0.15f + hand.forward * (0.1f + zoomOffset);
        textPlane.transform.position = hand.position + offset;

        // Always face the camera
        Vector3 toCamera = Camera.main.transform.position - textPlane.transform.position;
        Quaternion targetRot = Quaternion.LookRotation(-toCamera.normalized);
        textPlane.transform.rotation = Quaternion.Slerp(textPlane.transform.rotation, targetRot, Time.deltaTime * 10f);

        // Toggle zoom with Z key
        if (Input.GetKeyDown(KeyCode.Z))
        {
            isZoomed = !isZoomed;
        }
    }

    public void DisplayResponseText(string text)
    {
        if (textMeshPro != null)
        {
            textMeshPro.text =
                "<size=130%><b><color=#000000>VRKitchen Guide</color></b></size>\n\n" + text;
        }
    }
}
