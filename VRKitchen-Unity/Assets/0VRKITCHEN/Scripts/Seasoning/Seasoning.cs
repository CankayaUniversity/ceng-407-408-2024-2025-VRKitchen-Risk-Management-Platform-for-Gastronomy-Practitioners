using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seasoning : MonoBehaviour
{
    private string seasoningType;

    private void Start()
    {
        seasoningType = gameObject.tag;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Dish")) // Make sure the dish has the "Dish" tag
        {
            Dish dish = other.GetComponent<Dish>();
            if (dish != null)
            {
                dish.AddSeasoning(seasoningType, 1);
            }
            
            gameObject.SetActive(false);
        }
    }
}
