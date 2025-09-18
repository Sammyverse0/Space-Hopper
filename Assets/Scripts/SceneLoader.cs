using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public GameObject pausePanel;
    private bool isPaused = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Load(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void level1()
    {
        Load("Level1");
        Time.timeScale = 1f;
    }



    public void MainMenu()
    {
        Load("MainMenu");
    }

    public void settings()
    {
        Load("Settings");
    }
    public void quit()
    {
        Application.Quit();
    }



    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;  // Stops the game
            pausePanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;  // Resumes the game
            pausePanel.SetActive(false);
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }

    

    

}
