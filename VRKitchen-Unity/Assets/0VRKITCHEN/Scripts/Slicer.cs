using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
public class Slicer : MonoBehaviour
{
    public Material MaterialAfterSlice;
    public LayerMask sliceMask;

    public bool isTouched;

    private void Update()
    {
        if (isTouched)
        {
            isTouched = false;

            // OverlapBox ile kesilecek objeleri bul
            Collider[] objectsToBeSliced = Physics.OverlapBox(
                transform.position,
                new Vector3(0.5f, 0.5f, 0.5f), // Genişliği arttırıldı
                transform.rotation,
                sliceMask
            );

            foreach (Collider objectToBeSliced in objectsToBeSliced)
            {
                // Slice işlemini dene
                SlicedHull slicedObject = SliceObject(objectToBeSliced.gameObject, MaterialAfterSlice);

                if (slicedObject == null)
                {
                    Debug.LogWarning("Slice başarısız: " + objectToBeSliced.name);
                    continue; // Bu objeyi atla
                }

                // Yeni üst ve alt parçaları oluştur
                GameObject upperHullGameobject = slicedObject.CreateUpperHull(objectToBeSliced.gameObject, MaterialAfterSlice);
                GameObject lowerHullGameobject = slicedObject.CreateLowerHull(objectToBeSliced.gameObject, MaterialAfterSlice);

                upperHullGameobject.transform.position = objectToBeSliced.transform.position;
                lowerHullGameobject.transform.position = objectToBeSliced.transform.position;

                // Orijinal objede rigidbody varsa velocity al, yoksa Vector3.zero
                Rigidbody originalRb = objectToBeSliced.GetComponent<Rigidbody>();
                Vector3 originalVelocity = originalRb != null ? originalRb.velocity : Vector3.zero;

                // Fiziksel davranışları yeni parçalara uygula
                MakeItPhysical(upperHullGameobject, originalVelocity);
                MakeItPhysical(lowerHullGameobject, originalVelocity);

                // Orijinal objeyi yok et
                Destroy(objectToBeSliced.gameObject);
            }
        }
    }

    private void MakeItPhysical(GameObject obj, Vector3 _velocity)
    {
        // MeshCollider ekle (eğer yoksa) ve convex olarak ayarla
        MeshCollider meshCol;
        if (!obj.TryGetComponent<MeshCollider>(out meshCol))
        {
            meshCol = obj.AddComponent<MeshCollider>();
        }
        meshCol.convex = true;

        // Rigidbody ekle ve ayarlarını yap
        Rigidbody rb = obj.AddComponent<Rigidbody>();
        rb.velocity = -_velocity;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Gerekirse layer ayarla (kesilebilir objelerde tekrar kullanılabilsin diye)
        obj.layer = LayerMask.NameToLayer("Sliceable");

        // rb.AddForce(3 * new Vector3(randomNumberX, randomNumberY, randomNumberZ), ForceMode.Impulse);
    }

    private SlicedHull SliceObject(GameObject obj, Material crossSectionMaterial = null)
    {
        // slice the provided object using the transforms of this object
        return obj.Slice(transform.position, transform.up, crossSectionMaterial);
    }

}
