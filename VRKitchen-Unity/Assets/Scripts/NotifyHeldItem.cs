//using Oculus.Interaction;
//using UnityEngine;
//using UnityEngine.XR.Interaction.Toolkit;

//public class NotifyHeldItem : MonoBehaviour
//{
//    private void OnEnable()
//    {
//        var interactable = GetComponent<XRGrabInteractable>();
//        interactable.selectEntered.AddListener(OnGrab);
//        interactable.selectExited.AddListener(OnRelease);
//    }

//    private void OnDisable()
//    {
//        var interactable = GetComponent<XRGrabInteractable>();
//        interactable.selectEntered.RemoveListener(OnGrab);
//        interactable.selectExited.RemoveListener(OnRelease);
//    }

//    private void OnGrab(SelectEnterEventArgs args)
//    {
//        var heldItemManager = args.interactor.GetComponentInChildren<HeldItemManager>();
//        if (heldItemManager != null)
//        {
//            heldItemManager.AddItemTag(gameObject.tag);
//        }
//    }

//    private void OnRelease(SelectExitEventArgs args)
//    {
//        var heldItemManager = args.interactor.GetComponentInChildren<HeldItemManager>();
//        if (heldItemManager != null)
//        {
//            heldItemManager.RemoveItemTag(gameObject.tag);
//        }
//    }
//}
