using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus)) // '+'
            Increment();
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus)) // '-'
            Decrement();
        if (Input.GetKeyDown(KeyCode.K))
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
                    toAPI.queryText = $"I waited {waitedMinutes} minutes, what's the next step I should follow? Just give me the step with the step number without any explanation.";
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
