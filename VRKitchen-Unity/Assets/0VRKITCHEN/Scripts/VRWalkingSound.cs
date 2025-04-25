using UnityEngine;

public class VRWalkingSound : MonoBehaviour
{
    [SerializeField] private float movementThreshold = 0.05f; // 5 cm movement needed
    [SerializeField] private float stepInterval = 0.5f; // seconds between steps

    private Vector3 lastPosition;
    private float stepTimer = 0f;

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        stepTimer -= Time.deltaTime;

        float distanceMoved = Vector3.Distance(transform.position, lastPosition);

        if (distanceMoved >= movementThreshold && stepTimer <= 0f)
        {
            Debug.Log(" Player is moving - walking sound triggered");
            AudioController.Instance.PlayWalkingSound();
            stepTimer = stepInterval;

            lastPosition = transform.position; // move update here
        }
    }
}
