using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Random = UnityEngine.Random;

public class SaltShaker : MonoBehaviour
{
    [SerializeField] private GameObject saltPrefab;
    [SerializeField] private Transform sprinklePoint;
    [SerializeField] private int poolSize = 10;
    private Queue<GameObject> saltPool = new Queue<GameObject>();
    
    private bool isSprinkling = false;
    [SerializeField] private float sprinkleThreshold = -0.7f;

    private void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject salt = Instantiate(saltPrefab);
            salt.SetActive(false);
            saltPool.Enqueue(salt);
        }
    }

    private void Update()
    {
        if (GetComponent<XRGrabInteractable>().isSelected)
        {
            // Eğer tuzluk baş aşağıysa tuz dökülmeye başlasın
            if (transform.up.y < sprinkleThreshold && !isSprinkling)
            {
                isSprinkling = true;
                InvokeRepeating("SprinkleSalt", 0f, 0.1f);
            }
            else if (transform.up.y >= sprinkleThreshold && isSprinkling)
            {
                isSprinkling = false;
                CancelInvoke("SprinkleSalt");
            }
        }
        else
        {
            isSprinkling = false;
            CancelInvoke("SprinkleSalt");
        }
    }
    
    private void SprinkleSalt()
    {
        if (saltPool.Count > 0)
        {
            GameObject salt = saltPool.Dequeue();
            salt.transform.position = sprinklePoint.position + Random.insideUnitSphere * 0.05f;
            salt.SetActive(true);
            
            Debug.Log($"Salt sprinkled.");
            
            Rigidbody rb = salt.GetComponent<Rigidbody>();
            rb.velocity = new Vector3(Random.Range(-0.2f, 0.2f), -1f, Random.Range(-0.2f, 0.2f));
    
            StartCoroutine(ReturnSaltToPool(salt, 5f));
        }
        
    }
    
    IEnumerator ReturnSaltToPool(GameObject salt, float time)
    {
        yield return new WaitForSeconds(time);
        salt.SetActive(false);
        saltPool.Enqueue(salt);
    }
}
