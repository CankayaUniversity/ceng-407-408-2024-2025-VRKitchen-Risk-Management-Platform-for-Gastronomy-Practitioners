using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice; 
public class SliceTest : MonoBehaviour
{
    public Material MaterialAfterSlice;
    public LayerMask sliceMask;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            Collider[] objectsToSlice = Physics.OverlapBox(transform.position, new Vector3(1, 0.1f, 0.1f), transform.rotation, sliceMask);
            foreach (Collider objectToSlice in objectsToSlice)
            {
                //Slices the main object
                SlicedHull slicedObject = SliceObject(objectToSlice.gameObject, MaterialAfterSlice);

                //Creates the upper part  of the Slice
                GameObject upperHullGameobject = slicedObject.CreateUpperHull(objectToSlice.GetComponent<Collider>().gameObject, MaterialAfterSlice);

                //Creates the lower part of the Slice
                GameObject lowerHullGameobject = slicedObject.CreateLowerHull(objectToSlice.GetComponent<Collider>().gameObject, MaterialAfterSlice);


                MakeItPhysical(lowerHullGameobject);
                MakeItPhysical(upperHullGameobject);

                //Destroys the main object that was sliced
                Destroy(objectToSlice.gameObject);
            }
        }
    }

    private SlicedHull SliceObject(GameObject obj, Material crossSectionMaterial = null)
    {
        // slice the provided object using the transforms of this object
        return obj.Slice(transform.position, transform.up, crossSectionMaterial);
    }

    private void MakeItPhysical(GameObject obj, Material mat = null)
    {
        obj.AddComponent<MeshCollider>().convex = true;
        obj.AddComponent<Rigidbody>();     
    }
}
