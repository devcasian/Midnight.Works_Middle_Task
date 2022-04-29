using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadNightLevel()
    {
        SceneManager.LoadScene("LevelNight");
        Debug.Log("Loaded Night Level!");
    }

    public void LoadDayLevel()
    {
        SceneManager.LoadScene("LevelDay");
        Debug.Log("Loaded Day Level!");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Quit success!");
    }
}