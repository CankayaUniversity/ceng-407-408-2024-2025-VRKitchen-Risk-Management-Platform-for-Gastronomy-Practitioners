using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.InputSystem;
using System.Collections;

public class ClearMemoryHoldTrigger : MonoBehaviour
{
    [Header("Clear Memory API")]
    [SerializeField] private string clearMemoryUrl = "https://lviubjhdkcolf6ihebsg3aohf40ocbxs.lambda-url.eu-central-1.on.aws/clear_memory";

    [Header("Input Bindings (Assign in Inspector)")]
    public InputActionReference ButtonA; // e.g. Grip button
    public InputActionReference ButtonB; // e.g. Trigger button

    private float holdDuration = 3f;
    private float holdTimer = 0f;
    private bool isQueryInProgress = false;

    private void OnEnable()
    {
        ButtonA.action.Enable();
        ButtonB.action.Enable();
    }

    private void OnDisable()
    {
        ButtonA.action.Disable();
        ButtonB.action.Disable();
    }

    void Update()
    {
        bool aHeld = ButtonA.action.IsPressed();
        bool bHeld = ButtonB.action.IsPressed();

        if (aHeld && bHeld)
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdDuration && !isQueryInProgress)
            {
                StartCoroutine(ClearMemoryRequest());
                holdTimer = 0f;
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }

    private IEnumerator ClearMemoryRequest()
    {
        isQueryInProgress = true;
        Debug.Log("Holding buttons detected. Sending memory clear request...");

        UnityWebRequest req = UnityWebRequest.Get(clearMemoryUrl);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Memory cleared: " + req.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error clearing memory: " + req.error);
        }

        isQueryInProgress = false;
    }
}
