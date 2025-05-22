using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

public class PanFoodHolder : MonoBehaviour
{
    private List<Transform> stuckFoods = new List<Transform>();
    private float verticalSpacing = 0.025f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food") && other.attachedRigidbody != null)
        {
            var grab = other.GetComponent<XRGrabInteractable>();
            if (grab != null && grab.isSelected)
                return;

            if (!stuckFoods.Contains(other.transform))
            {
                stuckFoods.Add(other.transform);
                other.attachedRigidbody.isKinematic = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Food") && stuckFoods.Contains(other.transform))
        {
            stuckFoods.Remove(other.transform);

            var rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void LateUpdate()
    {
        // Tüm food objelerini sabit konum + sabit rotasyonla yerleştir
        for (int i = 0; i < stuckFoods.Count; i++)
        {
            Transform food = stuckFoods[i];
            if (food != null)
            {
                // Konumlandır
                Vector3 basePos = transform.position + Vector3.up * (0.05f + i * verticalSpacing);
                food.position = basePos;

                // Sabit rotasyon: yatık (90, 0, 0)
                food.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }
    }
}