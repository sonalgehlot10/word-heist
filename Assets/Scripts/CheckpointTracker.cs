using UnityEngine;

public class CheckpointTracker : MonoBehaviour
{
    public static CheckpointTracker Instance { get; private set; }

    public int currentCheckpointIndex = 0;

    [Tooltip("Set this per level e.g. 'Level1', 'Level2', 'Tutorial'")]
    public string levelName = "Level1";

    private GoogleFormAnalytics gfAnalytics;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        gfAnalytics = FindObjectOfType<GoogleFormAnalytics>();
    }

    public void LogAttempt()
    {
        if (gfAnalytics == null) return;
        gfAnalytics.LogEvent("Checkpoint/Attempt", levelName + "_Checkpoint_" + currentCheckpointIndex);
    }
}