using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCross : MonoBehaviour
{
    public List<GameObject> touchingItems = new List<GameObject>();
    public bool isContamination;

    public UnityToAPI toAPI;

    private bool hasSentContaminationQuery = false;
    private bool contaminationCoroutineRunning = false;

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
        bool hasChicken = touchingItems.Exists(obj =>
            obj != null && obj.name.Contains("Chicken Prefab(Clone)"));

        bool hasSteak = touchingItems.Exists(obj =>
            obj != null && obj.name.Contains("Steak Prefab(Clone)"));

        if (!isContamination && hasChicken && hasSteak && !contaminationCoroutineRunning)
        {
            StartCoroutine(HandleContamination());
        }
        else if (isContamination && (!hasChicken || !hasSteak || touchingItems.Count == 0))
        {
            isContamination = false;
            hasSentContaminationQuery = false;
            contaminationCoroutineRunning = false;
        }
    }

    private IEnumerator HandleContamination()
    {
        contaminationCoroutineRunning = true;

        Debug.Log("Cross Contamination Detected!");

        foreach (var item in touchingItems)
        {
            item.tag = "Trash";
        }

        if (visualFeedback != null && contaminationMarkerTransform != null)
        {
            visualFeedback.ShowExclamation(contaminationMarkerTransform.position);
        }

        if (!hasSentContaminationQuery && toAPI != null)
        {
            StartCoroutine(DelayedContaminationQuery());
            hasSentContaminationQuery = true;
        }

        isContamination = true;
        yield break;
    }

    private IEnumerator DelayedContaminationQuery()
    {
        yield return new WaitForSeconds(5f); // Delay to allow other queries to finish

        toAPI.queryText = RagCommands.CrossContaminationQuery;
        toAPI.SubmitQuery();
        Debug.Log(toAPI.queryText);
    }
    
        public void ResetContamination()
        {
            isContamination = false;
            hasSentContaminationQuery = false;
            contaminationCoroutineRunning = false;

            if (visualFeedback != null)
            {
                visualFeedback.HideExclamation();
            }

            Debug.Log("Board contamination manually cleared.");
        }

}
