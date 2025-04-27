using UnityEngine;
using UnityEngine.InputSystem;

public class VRWalkingSound : MonoBehaviour
{
    [Header("Tracking")]
    [SerializeField] private Transform trackedTransform;
    [SerializeField] private InputActionProperty moveInput;

    [Header("Footstep Settings")]
    [SerializeField] private float minSpeed = 0.15f;
    [SerializeField] private float stepInterval = 0.5f;
    [SerializeField] private float stopGracePeriod = 0.3f;

    private float stepTimer = 0f;
    private float lastInputTime = 0f;

    private Vector3 lastPosition;
    private bool isWalking = false;

    private void Start()
    {
        lastPosition = trackedTransform.position;
    }

    private void Update()
    {
        stepTimer -= Time.deltaTime;

        Vector3 currentPosition = trackedTransform.position;
        float velocity = (currentPosition - lastPosition).magnitude / Time.deltaTime;
        lastPosition = currentPosition;

        Vector2 moveValue = moveInput.action.ReadValue<Vector2>();
        bool joystickActive = moveValue.magnitude > 0.1f;

        if (joystickActive)
        {
            lastInputTime = Time.time;
        }

        bool shouldBeWalking = joystickActive && velocity > minSpeed;

        // START WALKING
        if (shouldBeWalking)
        {
            if (!isWalking)
            {
                isWalking = true;
                Debug.Log("Walking started");
            }

            if (stepTimer <= 0f)
            {
                AudioController.Instance.PlayWalkingSound();
                Debug.Log("Step played: walking actively");
                stepTimer = stepInterval;
            }
        }

        //  STOP WALKING if joystick hasnt been touched recently
        if (isWalking && (Time.time - lastInputTime > stopGracePeriod))
        {
            isWalking = false;
            stepTimer = 0f;
            Debug.Log(" Walking stopped (joystick released)");
        }
    }
}
