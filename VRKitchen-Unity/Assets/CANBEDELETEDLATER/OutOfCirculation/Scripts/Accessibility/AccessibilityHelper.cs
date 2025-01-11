using System.Collections;
using UnityEngine;

public class AccessibilityHelper : MonoBehaviour
{
    static AccessibilityHelper s_Instance;

    bool m_IsOn;
    bool m_CoroutineStarted = false;

    public static void ToggleTTS(bool on)
    {
        if (s_Instance == null)
        {
            var go = new GameObject("AccessibilityHelper");
            s_Instance = go.AddComponent<AccessibilityHelper>();
        }

        s_Instance.m_IsOn = on;

        if (s_Instance.m_IsOn)
        {
            UAP_AccessibilityManager.EnableAccessibility(true);
            UAP_AccessibilityManager.Say("UI Narration on", false);
        }
        else
        {
            if(!s_Instance.m_CoroutineStarted)
                s_Instance.StartCoroutine(s_Instance.DisableCoroutine());
        }
    }

    IEnumerator DisableCoroutine()
    {
        m_CoroutineStarted = true;
        //this could have been switch on AGAIN before the wait was finished, so we make sure before saying it's off
        UAP_AccessibilityManager.Say("UI Narration off", false);

        
        yield return new WaitForSecondsRealtime(3.0f);

        if (!m_IsOn)
        {
            UAP_AccessibilityManager.EnableAccessibility(false);
        }
        
        m_CoroutineStarted = false;
    }
}
