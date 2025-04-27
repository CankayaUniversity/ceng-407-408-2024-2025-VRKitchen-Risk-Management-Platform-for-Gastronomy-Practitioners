using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FoodDispenser : MonoBehaviour
{
    public FoodData foodData;

    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        if (grabInteractable != null)
            grabInteractable.selectEntered.AddListener(OnGrabbed);
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (foodData != null)
        {
            GameObject spawnedFood = Instantiate(foodData.foodPrefab, transform.position, transform.rotation);

            var spawnedInteractable = spawnedFood.GetComponent<XRGrabInteractable>();
            if (spawnedInteractable != null)
            {
                var interactor = args.interactorObject;
                interactor.transform.TryGetComponent(out XRDirectInteractor directInteractor);

                if (directInteractor != null)
                {
                    spawnedInteractable.interactionManager = grabInteractable.interactionManager;
                    directInteractor.interactionManager.SelectEnter(directInteractor, spawnedInteractable);
                }
            }
        }
    }
}