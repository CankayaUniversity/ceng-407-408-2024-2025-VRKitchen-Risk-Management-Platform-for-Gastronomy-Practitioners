using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCross : MonoBehaviour
{
    public List<string> touchingItemName = new List<string>();
    public bool isContamination;
    //[SerializeField] private QuestSystem questSystem;
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
        if (isContamination && itemName!="Meat") 
        {
            //RAGA BURDA CONTAMÝNATÝON VAR DÝCEK
            Debug.Log("contamination var kardeþ");
        }
    }
    void CheckContamination()
    {
        if (!isContamination && touchingItemName.Contains("Meat"))
        {
            isContamination=true;
            
        }
       else if (touchingItemName.Count == 0)
        {
            isContamination = false;
        }

    }
}

