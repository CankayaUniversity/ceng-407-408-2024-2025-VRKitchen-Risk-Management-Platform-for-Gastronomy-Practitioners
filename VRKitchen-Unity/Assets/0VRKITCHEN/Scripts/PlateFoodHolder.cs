using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateFoodHolder : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag=="Food")
        {
            collision.transform.parent = transform;
        }
    }
}
