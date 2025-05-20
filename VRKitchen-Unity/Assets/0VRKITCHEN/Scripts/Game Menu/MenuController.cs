using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void LoadKitchenScene()
    {
        SceneManager.LoadScene("Simal_Final_Scene");
    }

    public void LoadSoundSettings()
    {
        SceneManager.LoadScene("SoundSettings");
    }

    public void LoadInstructionScene()
    {
        SceneManager.LoadScene("Instructions");
    }

    public void LoadCreditsScene()
    {
        SceneManager.LoadScene("Credits");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();
    }
}
