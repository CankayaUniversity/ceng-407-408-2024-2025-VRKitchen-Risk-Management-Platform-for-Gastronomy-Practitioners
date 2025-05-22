using System.Collections;
using UnityEngine;
using EzySlice;
using UnityEngine.XR.Interaction.Toolkit;

public class XRSliceObject : MonoBehaviour
{
    public Transform startSlicePoint;
    public Transform endSlicePoint;
    public VelocityEstimator velocityEstimator;
    public InteractionLayerMask sliceableLayer;

    public float cutForce = 1.0f;
    public float minSliceVelocity = 1.0f;

    private float sliceCooldown = 0.2f;
    private float lastSliceTime = -Mathf.Infinity;

    void FixedUpdate()
    {
        if (Time.time - lastSliceTime < sliceCooldown) return;

        bool hasHit = Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hit);
        if (hasHit)
        {
            GameObject target = hit.transform.gameObject;
            IXRInteractable interactable = target.GetComponent<IXRInteractable>();

            if (interactable != null && (sliceableLayer.value & interactable.interactionLayers.value) != 0)
            {
                Slice(target);
                lastSliceTime = Time.time;
            }
        }
    }

    public void Slice(GameObject target)
    {
        if (velocityEstimator == null) return;

        Vector3 velocity = velocityEstimator.GetVelocityEstimate();
        if (velocity.magnitude < minSliceVelocity) return;

        Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, velocity);
        planeNormal.Normalize();

        Material originalMaterial = null;
        MeshRenderer renderer = target.GetComponent<MeshRenderer>();
        if (renderer != null && renderer.material != null)
        {
            originalMaterial = renderer.material;
        }

        SlicedHull hull = target.Slice(endSlicePoint.position, planeNormal, originalMaterial);

        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(target, originalMaterial);
            GameObject lowerHull = hull.CreateLowerHull(target, originalMaterial);

            CopyComponentsFromOriginal(target, upperHull);
            CopyComponentsFromOriginal(target, lowerHull);

            string objectName = target.name.ToLower().Replace("(clone)", "").Trim();
            string action = $"I sliced the {objectName}. What is the next step?";

            GameActionManager manager = FindObjectOfType<GameActionManager>();
            if (manager != null)
            {
                manager.RegisterAction(action);
            }

            Destroy(target);
        }
    }

    private void CopyComponentsFromOriginal(GameObject original, GameObject slicedPart)
    {
        if (slicedPart == null) return;

        Rigidbody originalRb = original.GetComponent<Rigidbody>();
        XRGrabInteractable originalGrab = original.GetComponent<XRGrabInteractable>();

        Rigidbody rb = slicedPart.AddComponent<Rigidbody>();
        rb.mass = originalRb ? originalRb.mass : 1f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.AddExplosionForce(cutForce, slicedPart.transform.position, 1f);

        MeshCollider meshCollider = slicedPart.AddComponent<MeshCollider>();
        meshCollider.convex = true;

        if (originalGrab != null)
        {
            var grab = slicedPart.AddComponent<XRGrabInteractable>();

            grab.interactionLayers = originalGrab.interactionLayers;
            grab.movementType = originalGrab.movementType;
            grab.throwOnDetach = originalGrab.throwOnDetach;
            grab.trackPosition = originalGrab.trackPosition;
            grab.trackRotation = originalGrab.trackRotation;
            grab.throwVelocityScale = originalGrab.throwVelocityScale;
            grab.throwAngularVelocityScale = originalGrab.throwAngularVelocityScale;
        }

        slicedPart.layer = original.layer;
    }
}
