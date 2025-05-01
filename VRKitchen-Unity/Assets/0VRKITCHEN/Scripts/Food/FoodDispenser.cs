using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FoodDispenser : MonoBehaviour
{
    public FoodData foodData;

    private XRBaseInteractable dispenserInteractable;

    private void Awake()
    {
        dispenserInteractable = GetComponent<XRBaseInteractable>();
    }

    private void OnEnable()
    {
        if (dispenserInteractable != null)
            dispenserInteractable.selectEntered.AddListener(OnGrabAttempted);
    }

    private void OnDisable()
    {
        if (dispenserInteractable != null)
            dispenserInteractable.selectEntered.RemoveListener(OnGrabAttempted);
    }

    private void OnGrabAttempted(SelectEnterEventArgs args)
    {
        if (foodData == null || foodData.foodPrefab == null)
            return;

        // Get the interactor (left or right hand)
        if (args.interactorObject is XRDirectInteractor directInteractor)
        {
            // Use the hand's position and rotation to spawn the food
            Transform handTransform = directInteractor.transform;
            GameObject spawnedFood = Instantiate(
                foodData.foodPrefab,
                handTransform.position,
                handTransform.rotation
            );

            XRGrabInteractable spawnedInteractable = spawnedFood.GetComponent<XRGrabInteractable>();

            if (spawnedInteractable != null)
            {
                spawnedInteractable.interactionManager = dispenserInteractable.interactionManager;

                // Force the hand to release dispenser and grab the new food
                directInteractor.interactionManager.SelectExit(directInteractor, dispenserInteractable);
                directInteractor.interactionManager.SelectEnter(directInteractor, spawnedInteractable);
            }
        }
    }
}