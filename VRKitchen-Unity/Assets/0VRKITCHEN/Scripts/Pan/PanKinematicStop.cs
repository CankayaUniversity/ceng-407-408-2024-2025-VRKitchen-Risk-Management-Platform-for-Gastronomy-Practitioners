using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

public class PanFoodHolder : MonoBehaviour
{
    private class FoodData
    {
        public Transform transform;
        public float heightOffset;
        public Quaternion baseRotation;
        public Vector3 localOffset;
        public Collider collider;

        public FoodData(Transform t, float h, Quaternion r, Vector3 o, Collider c)
        {
            transform = t;
            heightOffset = h;
            baseRotation = r;
            localOffset = o;
            collider = c;
        }
    }

    private List<FoodData> stuckFoods = new List<FoodData>();

    [SerializeField] private float placementRadius = 0.12f;
    [SerializeField] private float randomTiltRange = 10f;
    [SerializeField] private float verticalOffset = 0.01f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Food") || other.attachedRigidbody == null) return;

        var grab = other.GetComponent<XRGrabInteractable>();
        if (grab != null && grab.isSelected) return;

        if (stuckFoods.Exists(f => f.transform == other.transform)) return;

        Rigidbody rb = other.attachedRigidbody;
        Collider col = other.GetComponent<Collider>();
        if (col == null) return;

        // Fizik ve çarpýþmayý kapat
        rb.isKinematic = true;
        rb.detectCollisions = false;
        col.enabled = false;

        float height = GetObjectHeight(other.transform);

        float y = Random.Range(-randomTiltRange, randomTiltRange);
        float z = Random.Range(-randomTiltRange, randomTiltRange);
        Quaternion tilt = Quaternion.Euler(90f, y, z);

        Vector2 circle = Random.insideUnitCircle * placementRadius;
        Vector3 offset = new Vector3(circle.x, 0, circle.y);

        stuckFoods.Add(new FoodData(other.transform, height, tilt, offset, col));
    }

    private void OnTriggerExit(Collider other)
    {
        var data = stuckFoods.Find(f => f.transform == other.transform);
        if (data != null)
        {
            stuckFoods.Remove(data);

            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.detectCollisions = true;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (data.collider != null)
                data.collider.enabled = true;
        }
    }

    private void LateUpdate()
    {
        foreach (var food in stuckFoods)
        {
            if (food.transform == null) continue;

            Vector3 pos = transform.position + food.localOffset + Vector3.up * (food.heightOffset / 2f + verticalOffset);
            food.transform.position = pos;
            food.transform.rotation = food.baseRotation;
        }
    }

    private float GetObjectHeight(Transform obj)
    {
        Renderer renderer = obj.GetComponentInChildren<Renderer>();
        return renderer != null ? renderer.bounds.size.y : 0.05f;
    }
}
