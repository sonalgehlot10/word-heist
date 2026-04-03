using UnityEngine;

public class TimedHint : MonoBehaviour
{
    [Tooltip("Seconds to wait before showing the hint")]
    public float startDelay = 3f;

    [Tooltip("How many seconds to keep the hint visible")]
    public float displayDuration = 5f;

    void Start()
    {
        gameObject.SetActive(false);
        Invoke(nameof(ShowHint), startDelay);
    }

    void ShowHint()
    {
        gameObject.SetActive(true);
        Invoke(nameof(HideHint), displayDuration);
    }

    void HideHint()
    {
        gameObject.SetActive(false);
    }
}
