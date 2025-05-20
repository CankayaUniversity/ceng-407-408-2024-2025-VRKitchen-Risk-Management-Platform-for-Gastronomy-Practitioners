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

        if (args.interactorObject is XRDirectInteractor directInteractor)
        {
            Transform handTransform = directInteractor.transform;
            GameObject spawnedFood = Instantiate(
                foodData.foodPrefab,
                handTransform.position,
                handTransform.rotation
            );
            
            FoodInstance foodInstance = spawnedFood.GetComponent<FoodInstance>();
            if (foodInstance != null)
            {
                foodInstance.ResetCookingState(); 
            }

            XRGrabInteractable spawnedInteractable = spawnedFood.GetComponent<XRGrabInteractable>();

            if (spawnedInteractable != null)
            {
                spawnedInteractable.interactionManager = dispenserInteractable.interactionManager;

                // Force the hand to release dispenser and grab the new food
                directInteractor.interactionManager.SelectExit(directInteractor, dispenserInteractable);
                directInteractor.interactionManager.SelectEnter(directInteractor, spawnedInteractable);
            }

            // Send RAG query here
            GameActionManager actionManager = FindObjectOfType<GameActionManager>();
            if (actionManager != null)
            {
                string actionText = $"I picked up a {foodData.foodName}";
                actionManager.RegisterAction(actionText);
            }
        }
    }

}