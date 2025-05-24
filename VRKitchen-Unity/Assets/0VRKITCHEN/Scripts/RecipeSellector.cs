using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class RecipeSelector : MonoBehaviour
{
    public GameObject clipBoard;
    public TextMeshPro recipeListText;
    public UnityToAPI unityToAPI;
    public TextToSpeech textToSpeech;

    [Header("Input Actions")]
    public InputActionReference SelectUp;
    public InputActionReference SelectDown;
    public InputActionReference ToggleAndConfirm;

    private string[] recipes = { "Steak and Potatoes", "Chicken and Potatoes", "Hamburger" };
    private int currentIndex = 0;
    private float inputCooldown = 0.3f;
    private float lastInputTime = 0f;

    private string[] welcomeOptions = { "Start Cooking", "Tutorial" };
    private int welcomeIndex = 0;

    private enum State { Welcome, TutorialPlaying, Selecting, Hidden }
    private State currentState = State.Welcome;

    private bool selectionLocked = false;


    void OnEnable()
    {
        SelectUp.action.Enable();
        SelectDown.action.Enable();
        ToggleAndConfirm.action.Enable();
    }

    void OnDisable()
    {
        SelectUp.action.Disable();
        SelectDown.action.Disable();
        ToggleAndConfirm.action.Disable();
    }

    void Start()
    {
        ShowWelcomeMessage();
    }

    void Update()
    {

        if (selectionLocked)
            return; // Skip all input handling if locked

        if (currentState == State.Welcome && Time.time - lastInputTime >= inputCooldown)
        {
            if (SelectUp.action.triggered || Input.GetKeyDown(KeyCode.UpArrow))
            {
                welcomeIndex = (welcomeIndex - 1 + welcomeOptions.Length) % welcomeOptions.Length;
                UpdateWelcomeDisplay();
                lastInputTime = Time.time;
            }
            else if (SelectDown.action.triggered || Input.GetKeyDown(KeyCode.DownArrow))
            {
                welcomeIndex = (welcomeIndex + 1) % welcomeOptions.Length;
                UpdateWelcomeDisplay();
                lastInputTime = Time.time;
            }
        }

        if ((ToggleAndConfirm.action.triggered || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (currentState == State.Welcome)
            {
                if (welcomeOptions[welcomeIndex] == "Tutorial")
                    PlayTutorial();
                else
                    StartSelection();
            }
            else if (currentState == State.Selecting)
            {
                ConfirmSelection();
            }
        }

        if (currentState == State.Selecting && Time.time - lastInputTime >= inputCooldown)
        {
            if (SelectUp.action.triggered || Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveSelection(-1);
            }
            else if (SelectDown.action.triggered || Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveSelection(1);
            }
        }
    }

    private readonly string tutorialMessage = "Hello, welcome to VR Kitchen. I am your virtual assistant, here to support you throughout your cooking experience. During your time in the kitchen, you may encounter certain risks such as fire hazards, cross contamination, or water spillage. In the event of a fire, which may occur if the stove is left on without supervision, the player must quickly locate the fire extinguisher, aim at the flames, and spray using a sweeping motion to extinguish the fire safely. Cross-contamination happens when different raw meats are placed on the same cutting board. To resolve this, the contaminated food should be thrown away, the cutting board must be cleaned with a sponge and water, and the player should wash their hands before continuing. In the case of a water spillage on the kitchen floor, the player needs to use a mop to clean the area to prevent slipping hazards and maintain a safe working environment. Now you may start your experience. ";

    private void PlayTutorial()
    {
        currentState = State.TutorialPlaying;
        recipeListText.text = "Playing tutorial...";

        textToSpeech.onSpeechComplete = StartSelection;
        textToSpeech.Speak(tutorialMessage);
    }

    private void ShowWelcomeMessage()
    {
        clipBoard.SetActive(true);
        currentState = State.Welcome;
        welcomeIndex = 0;
        UpdateWelcomeDisplay();
    }

    private void UpdateWelcomeDisplay()
    {
        string display = "<b>Welcome to VR Kitchen!</b>\n\nUse the joystick to choose:\n\n";
        for (int i = 0; i < welcomeOptions.Length; i++)
        {
            display += (i == welcomeIndex) ? $"> {welcomeOptions[i]}\n" : $"  {welcomeOptions[i]}\n";
        }
        recipeListText.text = display;
    }

    private void StartSelection()
    {
        currentIndex = 0;
        currentState = State.Selecting;
        UpdateRecipeDisplay();
    }

    private void ConfirmSelection()
    {
        string selected = recipes[currentIndex];
        Debug.Log($"Selected Recipe: {selected}");

        unityToAPI.queryText = $"How to make {selected} in the game?";
        unityToAPI.SubmitQuery();

        recipeListText.text = "Loading steps for " + selected + "...";
        currentState = State.Hidden;
        selectionLocked = true; // ✅ Lock further access
    }

    private void MoveSelection(int direction)
    {
        currentIndex = (currentIndex + direction + recipes.Length) % recipes.Length;
        UpdateRecipeDisplay();
        lastInputTime = Time.time;
    }

    private void UpdateRecipeDisplay()
    {
        string display = "Please choose:\n\n";
        for (int i = 0; i < recipes.Length; i++)
        {
            display += (i == currentIndex) ? $"> {recipes[i]}\n" : $"  {recipes[i]}\n";
        }
        recipeListText.text = display;
    }
}
