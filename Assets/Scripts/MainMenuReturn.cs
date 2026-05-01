using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuReturn : MonoBehaviour
{
    public string mainMenuSceneName = "MainMenu";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CheckpointTimeAnalytics.Instance?.CompleteLevel("level_exit");
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    // Callable from a UI button
    public void GoToMainMenu()
    {
        CheckpointTimeAnalytics.Instance?.CompleteLevel("level_exit");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}