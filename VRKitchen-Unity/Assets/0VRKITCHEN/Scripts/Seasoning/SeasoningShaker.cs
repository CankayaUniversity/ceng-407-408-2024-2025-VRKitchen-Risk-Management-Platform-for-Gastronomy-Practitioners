using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Random = UnityEngine.Random;

public class SeasoningShaker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject seasoningPrefab;   
    [SerializeField] private Transform sprinklePoint;      

    [Header("Sprinkle Settings")]
    [SerializeField] private int poolSize = 10;            
    [SerializeField] private float sprinkleRate = 0.1f;    
    [SerializeField] private float minTiltAngle = 60f;     

    private Queue<GameObject> seasoningPool = new Queue<GameObject>();
    private float nextSprinkleTime = 0f;
    private bool isSprinkling = false;

    private void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject seasoning = Instantiate(seasoningPrefab);
            seasoning.SetActive(false);
            seasoningPool.Enqueue(seasoning);
        }
    }

    private void Update()
    {
        if (GetComponent<XRGrabInteractable>().isSelected)
        {
            float tiltAngle = Vector3.Angle(transform.up, Vector3.up);

            if (tiltAngle > minTiltAngle)
            {
                isSprinkling = true;
            }
            else
            {
                isSprinkling = false;
            }

            // Sprinkle seasoning at a fixed interval
            if (isSprinkling && Time.time >= nextSprinkleTime)
            {
                nextSprinkleTime = Time.time + sprinkleRate;
                SprinkleSalt();
            }
        }
        else
        {
            isSprinkling = false;
        }
    }
    
    private void SprinkleSalt()
    {
        if (seasoningPool.Count > 0)
        {
            GameObject salt = seasoningPool.Dequeue();
            salt.transform.position = sprinklePoint.position + Random.insideUnitSphere * 0.05f;
            salt.SetActive(true);
            
            //Debug.Log($"Salt sprinkled.");
            
            Rigidbody rb = salt.GetComponent<Rigidbody>();
            rb.velocity = new Vector3(Random.Range(-0.2f, 0.2f), -1f, Random.Range(-0.2f, 0.2f));
    
            StartCoroutine(ReturnSaltToPool(salt, 5f));
        }
        
    }
    
    IEnumerator ReturnSaltToPool(GameObject seasoning, float time)
    {
        yield return new WaitForSeconds(time);
        seasoning.SetActive(false);
        seasoningPool.Enqueue(seasoning);
    }
}
