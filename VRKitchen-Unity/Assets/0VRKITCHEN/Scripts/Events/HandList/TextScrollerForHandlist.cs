using UnityEngine;
using TMPro;

public class MeshTextScroller : MonoBehaviour
{
    public TextMeshPro textMesh;
    public float scrollSpeed = 20f;

    private float scrollOffset = 0f;
    private float targetOffset = 0f;
    private float maxScrollOffset = 0f;

    void Start()
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshPro>();

        UpdateScrollLimit();
    }

    void Update()
    {
        if (textMesh == null) return;

        // Example scroll control (mouse wheel or VR input)
        float input = Input.GetAxis("Mouse ScrollWheel"); // replace this with VR joystick input if needed
        targetOffset += input * scrollSpeed;

        // Clamp to prevent overscroll
        targetOffset = Mathf.Clamp(targetOffset, 0, maxScrollOffset);

        // Smooth scroll
        scrollOffset = Mathf.Lerp(scrollOffset, targetOffset, Time.deltaTime * 10f);

        // Offset the mesh
        textMesh.rectTransform.localPosition = new Vector3(0, scrollOffset, 0);
    }

    void UpdateScrollLimit()
    {
        // Estimate scroll limit (can also use bounds instead)
        float lineHeight = textMesh.fontSize * 1.2f;
        int totalLines = textMesh.text.Split('\n').Length;
        maxScrollOffset = Mathf.Max(0, totalLines * lineHeight - textMesh.rectTransform.rect.height);
    }

    public void SetText(string newText)
    {
        textMesh.text = newText;
        UpdateScrollLimit();
        scrollOffset = targetOffset = 0;
    }
}
