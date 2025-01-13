using Unity.XR.CoreUtils;
using UnityEngine;

[RequireComponent(typeof(CharacterController)), RequireComponent(typeof(XROrigin))]
public class WallPreventer : MonoBehaviour
{
    CharacterController m_CharacterController;
    Transform m_LocalHeadTransform;

    void Awake()
    {
        if (!TryGetComponent(out m_CharacterController) || !TryGetComponent(out XROrigin xrOrigin))
        {
            Debug.LogWarning("Missing Components. Disabling Now.");
            this.enabled = false;
            return;
        }
        m_LocalHeadTransform = xrOrigin.Camera.transform;
    }

    void Update()
    {
        // Set the Character Controller's center to the local position of the head transform on X and Z.
        m_CharacterController.center = new Vector3(m_LocalHeadTransform.localPosition.x, m_CharacterController.center.y, m_LocalHeadTransform.localPosition.z);

        // This line is important, otherwise the physics on the Character Controller won't update per frame.
        m_CharacterController.SimpleMove(Vector3.zero);
    }
}