using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuController : MonoBehaviour
{
    [Header("Scene Names")]
    public string tutorialSceneName = "TutorialLevel";
    public string level1SceneName   = "Alpha Level 1";
    public string level2SceneName   = "Alpha Level 2";

    [Header("Optional")]
    public TMP_Text subtitleText;

    
    public void OnLevel2Pressed()
    {
        SceneManager.LoadScene(level2SceneName);
    }
    

    public void OnLevel1Pressed()
    {
        SceneManager.LoadScene(level1SceneName);
    }

    public void OnTutorialPressed()
    {
        SceneManager.LoadScene(tutorialSceneName);
    }
    

    public void OnQuitPressed()
    {
        Application.Quit();
    }

}
