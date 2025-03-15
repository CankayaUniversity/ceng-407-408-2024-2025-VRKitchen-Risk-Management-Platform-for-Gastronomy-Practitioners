using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class RouteManager : MonoBehaviour
{
    [System.Serializable]
    public class TeleportPoint
    {
        public Collider triggerZone;
        public Transform teleportTarget;
    }

    public GameObject targetObject;
    public TeleportPoint[] teleportPoints;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("a");
        foreach (var point in teleportPoints)
        {
            if (other == point.triggerZone)
            {
                if (targetObject != null && point.teleportTarget != null)
                {
                    targetObject.transform.position = point.teleportTarget.position;
                }
            }
        }
    }
}