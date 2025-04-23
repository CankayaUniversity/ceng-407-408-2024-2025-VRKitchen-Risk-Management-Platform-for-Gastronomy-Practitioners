using UnityEngine;

public class CuttableWater : MonoBehaviour
{
    public float maxWaterLength = 2f; // Max height of water stream
    public LayerMask cuttableLayer;   // Layers that can cut the water

    private Transform waterMesh;      // Visual part (child cylinder)
    private Vector3 initialScale;

    void Start()
    {
        waterMesh = transform.GetChild(0); // Assume child 0 is the cylinder
        initialScale = waterMesh.localScale;
    }

    void Update()
    {
        // Raycast from top of the water object downward
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        float visibleLength = maxWaterLength;

        if (Physics.Raycast(ray, out hit, maxWaterLength, cuttableLayer))
        {
            visibleLength = hit.distance;
        }

        // Scale mesh only on Y
        float newY = visibleLength / maxWaterLength * initialScale.y;
        waterMesh.localScale = new Vector3(initialScale.x, newY, initialScale.z);

        // Move mesh so that base stays at the same place
        float offsetY = (maxWaterLength - visibleLength) / 2;
        waterMesh.localPosition = new Vector3(0, -offsetY, 0);
    }
}
