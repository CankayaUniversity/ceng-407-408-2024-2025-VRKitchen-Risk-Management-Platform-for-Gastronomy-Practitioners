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
        // Toggle clipboard with F9
        if (Input.GetKeyDown(KeyCode.F10))
        {
            isVisible = !isVisible;
            clipBoard.SetActive(isVisible);
        }

        // Follow hand and face the camera
        if (rightControllerVisual != null)
        {
            Transform hand = rightControllerVisual.transform;
            textPlane.transform.position = hand.position + hand.up * 0.15f;

            Vector3 toCamera = Camera.main.transform.position - textPlane.transform.position;
            textPlane.transform.forward = -toCamera.normalized;
        }
    }

    public void DisplayResponseText(string text)
    {
        if (textMeshPro != null)
        {
            textMeshPro.text = text;
        }
    }
}
