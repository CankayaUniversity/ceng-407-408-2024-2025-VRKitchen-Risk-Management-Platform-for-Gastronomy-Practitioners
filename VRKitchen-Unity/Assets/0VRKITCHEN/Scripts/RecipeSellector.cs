using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class RecipeSelector : MonoBehaviour
{
    public GameObject clipBoard; // Assign to ClipBoardVisual_2
    public TextMeshPro recipeListText;
    public UnityToAPI unityToAPI;

    [Header("Input Actions")]
    public InputActionReference SelectUp;
    public InputActionReference SelectDown;
    public InputActionReference ToggleAndConfirm;

    private string[] recipes = { "Steak and Potatoes", "Chicken and Potatoes", "Hamburger" };
    private int currentIndex = 0;
    private float inputCooldown = 0.3f;
    private float lastInputTime = 0f;

    private enum State { Welcome, Selecting, Hidden }
    private State currentState = State.Welcome;

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
        if (ToggleAndConfirm.action.triggered || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (currentState == State.Welcome)
            {
                StartSelection();
            }
            else if (currentState == State.Selecting)
            {
                ConfirmSelection();
            }
        }

        if (currentState != State.Selecting || Time.time - lastInputTime < inputCooldown)
            return;

        if (SelectUp.action.triggered || Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveSelection(-1);
        }
        else if (SelectDown.action.triggered || Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveSelection(1);
        }
    }

    private void ShowWelcomeMessage()
    {
        clipBoard.SetActive(true);
        recipeListText.text = "<b>Welcome to VR Kitchen!</b>\n\nPress the left controller joystick to choose a recipe.";
        currentState = State.Welcome;
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

        // ❌ Do not deactivate clipboard — HandTextDisplay controls it
        // clipBoard.SetActive(false);
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