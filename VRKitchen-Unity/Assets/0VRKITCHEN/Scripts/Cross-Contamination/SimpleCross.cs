using System.Collections.Generic;
using UnityEngine;

public class SimpleCross : MonoBehaviour
{
    public List<GameObject> touchingItems = new List<GameObject>();
    public bool isContamination;

    public UnityToAPI toAPI;
    

    private bool hasSentContaminationQuery = false;

    public VisualFeedbackController visualFeedback;
    public Transform contaminationMarkerTransform;
    

    private void Update()
    {
        CleanDestroyedItems();
        CheckContamination();
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject item = collision.gameObject;

        if (!touchingItems.Contains(item))
        {
            touchingItems.Add(item);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        GameObject item = collision.gameObject;

        if (touchingItems.Contains(item))
        {
            touchingItems.Remove(item);
        }
    }

    private void CleanDestroyedItems()
    {
        touchingItems.RemoveAll(item => item == null);
    }

    private void CheckContamination()
    {
        bool wasContaminated = isContamination;

        // Separate meat and non-meat items
        List<GameObject> meatItems = touchingItems.FindAll(obj =>
            obj != null && (obj.name.Contains("Steak Prefab(Clone)") || obj.name.Contains("Chicken Prefab(Clone)")));

        List<GameObject> nonMeatItems = touchingItems.FindAll(obj =>
            obj != null && !obj.name.Contains("Steak Prefab(Clone)") && !obj.name.Contains("Chicken Prefab(Clone)"));

        if (!isContamination && meatItems.Count > 0 && nonMeatItems.Count > 0)
        {
            isContamination = true;
            foreach (var item in touchingItems)
            {
                item.gameObject.tag = "Trash";
            }

            Debug.Log("Cross Contamination Detected!");

            if (visualFeedback != null && contaminationMarkerTransform != null)
            {
                visualFeedback.ShowExclamation(contaminationMarkerTransform.position);
            }

            if (!hasSentContaminationQuery && toAPI != null)
            {
                toAPI.queryText = GameQueries.CrossContaminationQuery;
                toAPI.SubmitQuery();
                Debug.Log(toAPI.queryText);
                hasSentContaminationQuery = true;
            }
        }
        else if (isContamination && (meatItems.Count == 0 || touchingItems.Count == 0))
        {
            isContamination = false;
            hasSentContaminationQuery = false;
        }
    }


    public void ResetContamination()
    {
        isContamination = false;
        hasSentContaminationQuery = false;

        // Removed HideExclamation from here too
        Debug.Log("Board contamination manually cleared.");
    }
}
