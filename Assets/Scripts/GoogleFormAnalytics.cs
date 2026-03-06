using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GoogleFormAnalytics : MonoBehaviour
{
    private string formURL = "https://docs.google.com/forms/d/e/1FAIpQLScQhpZOcvwU3VOD94tlHPknRO4_qeQ4MHeaYZQe42fsmW_n-g/formResponse";

    public void LogEvent(string eventName, string eventData)
    {
        StartCoroutine(PostToGoogle(eventName, eventData));
    }

    IEnumerator PostToGoogle(string eventName, string eventData)
    {
        WWWForm form = new WWWForm();

        form.AddField("entry.1226520711", eventName);
        form.AddField("entry.1093405768", eventData);

        UnityWebRequest www = UnityWebRequest.Post(formURL, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success || www.responseCode == 0)
        {
            Debug.Log("Google Form submission successful.");
        }
        else
        {
            Debug.LogError("Submission failed: " + www.error);
        }
    }
}