using UnityEngine;
using UnityEngine.InputSystem; // ✅ Required for InputActionReference


public class TimerActivator : MonoBehaviour
{
    public TimerPanel timerPanel;
    public InputActionReference toggleTimerAction;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) || toggleTimerAction.action.triggered)
        {
            timerPanel.TogglePanel();
        }
    }
}
