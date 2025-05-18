using UnityEngine;

public class TimerActivator : MonoBehaviour
{
    public TimerPanel timerPanel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            timerPanel.TogglePanel();
        }
    }
}
