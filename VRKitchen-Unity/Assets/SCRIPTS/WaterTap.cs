using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaterTap : MonoBehaviour
{
    [SerializeField] private GameObject waterLeak; // Assign the WaterLeak child object in the inspector
    [SerializeField] private float interactionDistance = 2f; // Distance required to interact
    [SerializeField] private TextMeshProUGUI interactionText; // Assign a UI Text object in the inspector
    private bool isLeaking = false;
    [SerializeField] private Transform player;

    void Start()
    {
        if (waterLeak != null)
            waterLeak.SetActive(false); // Ensure the leak is off at start

        interactionText.gameObject.SetActive(false); // Hide text at start
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform; // Find player
        }
        
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= interactionDistance) // Player is close enough
        {
            interactionText.gameObject.SetActive(true); // Show interaction text

            if (Input.GetKeyDown(KeyCode.E)) // Check if the player presses "E"
            {
                ToggleWaterLeak();
            }
        }
        else
        {
            interactionText.gameObject.SetActive(false); // Hide interaction text
        }
    }

    void ToggleWaterLeak()
    {
        isLeaking = !isLeaking; // Toggle state
        waterLeak.SetActive(isLeaking); // Enable or disable leak

        if (isLeaking)
            interactionText.text = "Press E to close the sink";
        else
            interactionText.text = "Press E to open the sink";
    }
}
