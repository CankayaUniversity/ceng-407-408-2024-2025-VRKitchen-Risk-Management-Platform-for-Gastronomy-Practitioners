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

        bool hasChicken = touchingItems.Exists(obj =>
            obj != null && obj.name.Contains("Chicken Prefab(Clone)"));

        bool hasSteak = touchingItems.Exists(obj =>
            obj != null && obj.name.Contains("Steak Prefab(Clone)"));

        // Only set contamination if both chicken and steak are present
        if (!isContamination && hasChicken && hasSteak)
        {
            isContamination = true;

            foreach (var item in touchingItems)
            {
                item.tag = "Trash";
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
        else if (isContamination && (!hasChicken || !hasSteak || touchingItems.Count == 0))
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
