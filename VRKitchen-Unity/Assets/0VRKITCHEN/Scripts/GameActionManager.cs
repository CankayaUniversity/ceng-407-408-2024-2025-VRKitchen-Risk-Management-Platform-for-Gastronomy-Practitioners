using UnityEngine;

public class GameActionManager : MonoBehaviour
{
    public UnityToAPI unityToAPI;

    public void RegisterAction(string playerAction)
    {
        if (unityToAPI != null)
        {
            unityToAPI.SubmitDynamicAction(playerAction);
        }
        else
        {
            Debug.LogWarning("UnityToAPI reference is missing in GameActionManager!");
        }
    }
}