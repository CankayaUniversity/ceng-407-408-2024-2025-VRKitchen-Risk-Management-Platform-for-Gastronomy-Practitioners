using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCross : MonoBehaviour
{
    public List<string> touchingItemName = new List<string>();
    public bool isContamination;

    public UnityToAPI toAPI;

    private bool hasSentContaminationQuery = false;

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
        // Cross-contamination = touching Meat and another object
        if (!isContamination && touchingItemName.Contains("Meat") && touchingItemName.Count > 1)
        {
            isContamination = true;
            Debug.Log("Cross Contamination Detected!");

            if (!hasSentContaminationQuery && toAPI != null)
            {
                toAPI.queryText = "Cross-contamination occurred in the game. What are the next safety steps to take?";
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
