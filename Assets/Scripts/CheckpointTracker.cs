using UnityEngine;

// Tracks which checkpoint segment the player is currently in
// and logs an attempt whenever they die in that segment.
public class CheckpointTracker : MonoBehaviour
{
    public static CheckpointTracker Instance { get; private set; }

    // Updated by CheckpointPortal when player passes through
    public int currentCheckpointIndex = 0;

    private GoogleFormAnalytics gfAnalytics;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        gfAnalytics = FindObjectOfType<GoogleFormAnalytics>();
    }

    // Called by every death source (spike, guard, energy)
    public void LogAttempt()
    {
        if (gfAnalytics == null) return;
        gfAnalytics.LogEvent("Checkpoint/Attempt", "Checkpoint_" + currentCheckpointIndex);
    }
}
