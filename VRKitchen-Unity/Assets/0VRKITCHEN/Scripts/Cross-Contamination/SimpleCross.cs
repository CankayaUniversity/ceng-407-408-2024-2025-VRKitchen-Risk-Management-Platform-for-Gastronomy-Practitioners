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

        bool containsMeat = touchingItems.Exists(obj => obj != null && obj.name.Contains("Meat"));
        bool moreThanOneItem = touchingItems.Count > 1;

        if (!isContamination && containsMeat && moreThanOneItem)
        {
            isContamination = true;
            Debug.Log("Cross Contamination Detected!");

            if (visualFeedback != null && contaminationMarkerTransform != null)
            {
                visualFeedback.ShowExclamation(contaminationMarkerTransform.position);
            }

            if (!hasSentContaminationQuery && toAPI != null)
            {
                toAPI.queryText = "Cross contamination happened in the game. What are the steps I should follow in the game only? Don't provide any other explanation";
                toAPI.SubmitQuery();
                hasSentContaminationQuery = true;
            }
        }
        else if (isContamination && (!containsMeat || touchingItems.Count <= 1))
        {
            isContamination = false;
            hasSentContaminationQuery = false;

            if (visualFeedback != null)
            {
                visualFeedback.HideExclamation();
            }

            Debug.Log("Contamination resolved.");
        }
    }
}
