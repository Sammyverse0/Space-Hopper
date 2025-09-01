using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Load(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void level1()
    {
        Load("Level1");
    }

    public void setting()
    {
        Load("Settings");
    }

}
