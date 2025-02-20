using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HandItemTracker : MonoBehaviour
{
    private XRDirectInteractor interactor;
    public List<string> heldItemTags = new List<string>();
    [SerializeField] private QuestSystem questSystem;
    private void Awake()
    {
        interactor = GetComponent<XRDirectInteractor>();
    }

    private void OnEnable()
    {
        interactor.selectEntered.AddListener(OnItemGrabbed);
        interactor.selectExited.AddListener(OnItemReleased);
    }

    private void OnDisable()
    {
        interactor.selectEntered.RemoveListener(OnItemGrabbed);
        interactor.selectExited.RemoveListener(OnItemReleased);
    }

    private void OnItemGrabbed(SelectEnterEventArgs args)
    {
        if (args.interactableObject != null)
        {
            string itemTag = args.interactableObject.transform.tag;
            if (!heldItemTags.Contains(itemTag))
            {
                heldItemTags.Add(itemTag);
                
                questSystem.OnItemTaken(itemTag); 
            }
        }
    }

    private void OnItemReleased(SelectExitEventArgs args)
    {
        if (args.interactableObject != null)
        {
            string itemTag = args.interactableObject.transform.tag;
            if (heldItemTags.Contains(itemTag))
            {
                heldItemTags.Remove(itemTag);
                
                questSystem.OnItemReleased(itemTag);  
            }

        }
    }

    public List<string> GetHeldItemTags()
    {
        return new List<string>(heldItemTags);
    }
}
