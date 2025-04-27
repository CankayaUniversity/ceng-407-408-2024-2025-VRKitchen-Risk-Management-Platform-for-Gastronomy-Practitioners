using UnityEngine;

public class WaterRaycastCutter : MonoBehaviour
{
    public Transform waterObject; // "water" objesi (cylinder)
    public float maxLength = 0.02f; // Maksimum su uzunluðu
    public LayerMask cuttableLayer;

    private Vector3 initialScale;
    private Vector3 initialLocalPosition;

    void Start()
    {
        if (waterObject == null)
        {
            Debug.LogError("Water object is not assigned!");
            return;
        }

        initialScale = waterObject.localScale;
        initialLocalPosition = waterObject.localPosition;
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        //  Ray'i görünür yap
        Debug.DrawRay(transform.position, Vector3.down * maxLength, Color.red);

        float visibleLength = maxLength;

        if (Physics.Raycast(ray, out hit, maxLength, cuttableLayer))
        {
            visibleLength = hit.distance;
        }

        float newYScale = (visibleLength / maxLength) * initialScale.y;
        waterObject.localScale = new Vector3(initialScale.x, newYScale, initialScale.z);

        float yOffset = (initialScale.y - newYScale) * waterObject.GetComponent<MeshFilter>().mesh.bounds.size.y;
        waterObject.localPosition = new Vector3(initialLocalPosition.x, initialLocalPosition.y + yOffset, initialLocalPosition.z);
    }
}
