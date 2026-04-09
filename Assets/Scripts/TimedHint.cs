using UnityEngine;

// Attach this to the hint text GameObject.
// It hides itself after displayDuration seconds whenever it is activated.
public class TimedHint : MonoBehaviour
{
    [Tooltip("How many seconds to stay visible before hiding")]
    public float displayDuration = 5f;

    void OnEnable()
    {
        CancelInvoke(nameof(HideHint));
        Invoke(nameof(HideHint), displayDuration);
    }

    void HideHint()
    {
        gameObject.SetActive(false);
    }
}
