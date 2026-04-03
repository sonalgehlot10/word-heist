using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuController : MonoBehaviour
{
    [Header("Scene Names")]
    public string tutorialSceneName = "TutorialLevel";
    public string level1SceneName   = "Alpha Level 1";

    [Header("Optional")]
    public TMP_Text subtitleText;

    public void OnTutorialPressed()
    {
        SceneManager.LoadScene(tutorialSceneName);
    }

    public void OnLevel1Pressed()
    {
        SceneManager.LoadScene(level1SceneName);
    }

    public void OnQuitPressed()
    {
        Application.Quit();
    }
}
