using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class TimerPanel : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text timerText;
    public Button plusButton;
    public Button minusButton;
    public Button startButton;
    private int minutes = 0;
    private float remainingTime;
    private bool isCountingDown = false;
    public UnityToAPI toAPI;  // Assign this from the inspector
    public InputActionReference TimerPlus;
    public InputActionReference TimerMinus;
    public InputActionReference TimerStart;


    void Start()
    {
        panel.SetActive(false);
        plusButton.onClick.AddListener(Increment);
        minusButton.onClick.AddListener(Decrement);
        startButton.onClick.AddListener(StartTimer);
    }

    void Update()
    {
        // ⌨️ Keyboard shortcuts
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus) || TimerPlus.action.triggered) // '+'
            Increment();
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus) || TimerMinus.action.triggered) // '-'
            Decrement();
        if (Input.GetKeyDown(KeyCode.K) || TimerStart.action.triggered)
            StartTimer();

        if (isCountingDown)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = 0f; // ✅ Clamp at zero
                isCountingDown = false;
                AudioController.Instance.PlayKitchenTimerSound();

                if (toAPI != null)
                {
                    int waitedMinutes = Mathf.FloorToInt(minutes); // Store the selected time
                    toAPI.queryText = RagCommands.TimerSet(waitedMinutes);
                    Debug.Log(toAPI.queryText);
                    toAPI.SubmitQuery();
                }

                startButton.interactable = true;
            }

            int mins = Mathf.FloorToInt(remainingTime / 60);
            int secs = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = $"{mins:00}:{secs:00}";
        }
    }

    void Increment()
    {
        minutes++;
        UpdateTimerDisplay();
    }

    void Decrement()
    {
        if (minutes > 0) minutes--;
        UpdateTimerDisplay();
    }

    void StartTimer()
    {
        remainingTime = minutes * 60f;
        isCountingDown = true;
        startButton.interactable = false;
    }

    void UpdateTimerDisplay()
    {
        timerText.text = $"{minutes:00}:00";
    }

    public void TogglePanel()
    {
        if (isCountingDown)
            return; // ❌ Block toggling during countdown

        panel.SetActive(!panel.activeSelf);
    }

}
