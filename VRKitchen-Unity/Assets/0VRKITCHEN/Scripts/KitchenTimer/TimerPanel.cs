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
    public AudioSource alarmSound;

    private int minutes = 0;
    private float remainingTime;
    private bool isCountingDown = false;

    void Start()
    {
        panel.SetActive(false);
        plusButton.onClick.AddListener(() => { minutes++; UpdateTimerDisplay(); });
        minusButton.onClick.AddListener(() => { if (minutes > 0) minutes--; UpdateTimerDisplay(); });
        startButton.onClick.AddListener(StartTimer);
    }

    public void TogglePanel()
    {
        panel.SetActive(!panel.activeSelf);
    }

    void UpdateTimerDisplay()
    {
        timerText.text = $"{minutes:00}:00";
    }

    void StartTimer()
    {
        remainingTime = minutes * 60f;
        isCountingDown = true;
        startButton.interactable = false;
    }

    void Update()
    {
        if (isCountingDown)
        {
            remainingTime -= Time.deltaTime;
            int mins = Mathf.FloorToInt(remainingTime / 60);
            int secs = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = $"{mins:00}:{secs:00}";

            if (remainingTime <= 0)
            {
                isCountingDown = false;
                alarmSound.Play();
                startButton.interactable = true;
            }
        }
    }
}
