using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void LoadKitchenScene()
    {
        SceneManager.LoadScene(1);
    }

    public void LoadSoundSettings()
    {
        SceneManager.LoadScene(2);
    }

    public void LoadInstructionScene()
    {
        SceneManager.LoadScene(3);
    }

    public void LoadCreditsScene()
    {
        SceneManager.LoadScene(4);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();
    }
}