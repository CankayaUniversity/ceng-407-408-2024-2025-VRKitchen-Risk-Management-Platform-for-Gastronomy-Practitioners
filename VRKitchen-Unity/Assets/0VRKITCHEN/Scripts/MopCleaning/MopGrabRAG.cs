using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MopGrabRAG : MonoBehaviour
{
    public XRGrabInteractable mopGrab;
    public UnityToAPI toAPI;

    private bool hasSentGrabQuery = false;

    private void Start()
    {
        if (mopGrab != null)
        {
            mopGrab.selectEntered.AddListener(OnGrabbed);
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (!hasSentGrabQuery && toAPI != null)
        {
            Debug.Log("Mop grabbed by player.");
            toAPI.queryText = "I grabbed the mop. What now?";
            toAPI.SubmitQuery();
            hasSentGrabQuery = true;
        }
    }
}
