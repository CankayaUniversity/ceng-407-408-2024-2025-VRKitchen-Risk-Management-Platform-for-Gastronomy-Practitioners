using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCross : MonoBehaviour
{
    public List<string> touchingItemName = new List<string>();
    public bool isContamination;

    public UnityToAPI toAPI;

    private bool hasSentContaminationQuery = false;

    public VisualFeedbackController visualFeedback;
    public Transform contaminationMarkerTransform; // Drag the fixed object in Inspector

    private void Update()
    {
        CheckContamination();
    }

    private void OnCollisionEnter(Collision collision)
    {
        string itemName = collision.gameObject.name;
        if (!touchingItemName.Contains(itemName))
        {
            touchingItemName.Add(itemName);
        }
    }

    void CheckContamination()
    {
        if (!isContamination && touchingItemName.Contains("Meat") && touchingItemName.Count > 1)
        {
            isContamination = true;
            Debug.Log("Cross Contamination Detected!");

            if (visualFeedback != null && contaminationMarkerTransform != null)
            {
                // 💥 Spawn exactly above the board, no matter what
                visualFeedback.ShowExclamation(contaminationMarkerTransform.position);
            }

            if (!hasSentContaminationQuery && toAPI != null)
            {
                toAPI.queryText = "Cross contamination happened in the game. What are the steps I should follow?";
                toAPI.SubmitQuery();
                hasSentContaminationQuery = true;
            }
        }
        else if (touchingItemName.Count == 0)
        {
            isContamination = false;
            hasSentContaminationQuery = false;
        }
    }
}
