using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseMenuCanvas;

    private bool isPaused = false;

    public Transform playerCamera; 


    void Start()
    {
        pauseMenuCanvas.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        pauseMenuCanvas.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused)
        {
            // Position the menu in front of the player's view
            Vector3 forward = playerCamera.forward;
            forward.y = 0; // keep it horizontal

            pauseMenuCanvas.transform.position = playerCamera.position + forward.normalized * 2f + Vector3.up * -0.2f;
            pauseMenuCanvas.transform.LookAt(playerCamera);
            pauseMenuCanvas.transform.Rotate(0, 180f, 0); // flip to face the user
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuCanvas.SetActive(false);
        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
