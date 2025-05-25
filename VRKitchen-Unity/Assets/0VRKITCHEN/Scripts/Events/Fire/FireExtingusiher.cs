using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FireExtinguisher : MonoBehaviour
{
    public ParticleSystem sprayEffect; // Assign the spray particle system
    public XRSimpleInteractable button; // Assign the button interactable
    public XRGrabInteractable extinguisherGrab;
    public UnityToAPI toAPI; // Assign the RAG interface script

    public OvenController ovenController;
    private bool isSpraying = false;
    private bool hasSentGrabQuery = false;

    private FireController.FireSource myFireSource;
    private void Start()
    {
        if (button != null)
        {
            button.selectEntered.AddListener(OnButtonPressed);
            button.selectExited.AddListener(OnButtonReleased);
            Debug.Log("✅ Button listeners registered.");
        }
        else
        {
            Debug.LogWarning("❌ Button is not assigned in the inspector.");
        }

        if (extinguisherGrab != null)
        {
            extinguisherGrab.selectEntered.AddListener(OnGrabbed);
            Debug.Log("✅ Grab listener registered.");
        }
        else
        {
            Debug.LogWarning("❌ Extinguisher grab is not assigned in the inspector.");
        }

        if (sprayEffect != null)
        {
            sprayEffect.Stop();
        }
    }


    private void OnGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log("🔥 OnGrabbed() triggered.");

        if (!hasSentGrabQuery && toAPI != null)
        {
            Debug.Log("📤 Sending fire extinguisher grab query to API.");
            toAPI.queryText = "I grabbed the fire extinguisher from the cupboard in the game. What now?";
            toAPI.SubmitQuery();
            hasSentGrabQuery = true;
        }
        else if (hasSentGrabQuery)
        {
            Debug.Log("⚠️ Grab query already sent, skipping.");
        }
        else
        {
            Debug.LogWarning("❌ toAPI is null.");
        }
    }


    private void OnButtonPressed(SelectEnterEventArgs args)
    {
        isSpraying = true;
        if (sprayEffect != null)
        {
            AudioController.Instance.PlayFireExtinguisherSound();
            sprayEffect.Play(); // Start spraying
        }
    }

    private void OnButtonReleased(SelectExitEventArgs args)
    {
        isSpraying = false;
        if (sprayEffect != null)
        {
            sprayEffect.Stop(); // Stop spraying
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isSpraying && other.CompareTag("Fire")) // Check if the spray touches a fire
        {
            ExtinguishFire(other.gameObject);
        }
    }

    private void ExtinguishFire(GameObject fire)
    {
        FireInstance fireInstance = fire.GetComponent<FireInstance>();

        if (fireInstance != null && fireInstance.source != null)
        {
            Debug.Log("Fire extinguished via FireController: " + fire.name);
            FireController.Instance.ExtinguishFire(fireInstance.source);
        }
        else
        {
            Debug.LogWarning("FireInstance component missing on fire object.");
            Destroy(fire);
        }
    }

}