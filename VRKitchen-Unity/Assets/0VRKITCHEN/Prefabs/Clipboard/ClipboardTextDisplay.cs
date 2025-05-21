using TMPro;
using UnityEngine;

public class ClipboardTextDisplay : MonoBehaviour
{
    [Header("Setup")]
    public GameObject clipboardVisual; // Just the visual root (not the whole prefab)
    public Transform leftControllerVisual;
    public TextMeshProUGUI textMeshPro;

    [Header("Offset Settings")]
    public Vector3 offsetFromHand = new Vector3(-0.12f, 0.15f, 0.25f); // Adjusted left

    private bool isVisible = true;

    void Start()
    {
        if (clipboardVisual == null || leftControllerVisual == null || textMeshPro == null)
        {
            Debug.LogError("ClipboardTextDisplay: Missing required references!");
            return;
        }

        SetText("Clipboard ready!");
        SetVisibility(false);

        // OPTIONAL: Style your text
        textMeshPro.enableWordWrapping = true;
        textMeshPro.overflowMode = TextOverflowModes.Overflow;
        textMeshPro.enableAutoSizing = true;
        textMeshPro.fontSizeMin = 10f;
        textMeshPro.fontSizeMax = 50f;

        // Optional: Outline
        textMeshPro.fontMaterial.SetColor("_FaceColor", Color.white);
        textMeshPro.fontMaterial.SetColor("_OutlineColor", Color.black);
        textMeshPro.fontMaterial.SetFloat("_OutlineWidth", 0.2f);
    }

void Update()
{
    if (Input.GetKeyDown(KeyCode.F9))
    {
        isVisible = !isVisible;
        SetVisibility(isVisible);
    }

    if (!clipboardVisual.activeSelf) return;

    Vector3 offsetPos = leftControllerVisual.position +
                        leftControllerVisual.right * offsetFromHand.x +
                        leftControllerVisual.up * offsetFromHand.y +
                        leftControllerVisual.forward * offsetFromHand.z;

    clipboardVisual.transform.position = offsetPos;

    Vector3 toCam = (Camera.main.transform.position - clipboardVisual.transform.position).normalized;
    clipboardVisual.transform.rotation = Quaternion.LookRotation(toCam);
    clipboardVisual.transform.Rotate(90f, 0f, 0f); // This works for your board

    // ✅ Rotate text canvas to match clipboard
    if (textMeshPro != null)
    {
        textMeshPro.transform.rotation = clipboardVisual.transform.rotation;
    }
}


    public void SetText(string text)
    {
        if (textMeshPro != null)
        {
            textMeshPro.text = text;
            textMeshPro.ForceMeshUpdate();
        }
    }

    public void SetVisibility(bool visible)
    {
        isVisible = visible;

        if (clipboardVisual != null)
            clipboardVisual.SetActive(visible);
    }

    // You can call this externally just like DisplayResponseText()
    public void DisplayResponseText(string responseText)
    {
        SetText(responseText);
    }
}
