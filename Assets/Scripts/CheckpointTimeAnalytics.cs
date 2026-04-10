using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class LevelCheckpointConfig
{
    public string levelName;
    public int checkpointCount = 4;
}

[Serializable]
public class CheckpointTimeEvent
{
    public string sessionId;
    public string level;
    public int checkpointNumber;
    public string checkpointName;
    public float startTime;
    public float endTime;
    public float durationSeconds;
    public string reason;
}

public class CheckpointTimeAnalytics : MonoBehaviour
{
    public static CheckpointTimeAnalytics Instance { get; private set; }

    [Header("Level setup")]
    [Tooltip("Optional overrides. If empty, the script uses heuristics: Tutorial = 3, others = 4.")]
    public List<LevelCheckpointConfig> levelConfigs = new List<LevelCheckpointConfig>();

    [Header("Logging")]
    public bool verboseLogging = false;

    private GoogleFormAnalytics gfAnalytics;
    private readonly Dictionary<string, int> checkpointCountByLevel = new Dictionary<string, int>();

    private string sessionId;
    private string currentLevel;
    private int currentCheckpointNumber;
    private int expectedCheckpointCount;
    private float segmentStartTime;
    private bool trackingActive;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
            return;

        GameObject go = new GameObject("CheckpointTimeAnalytics");
        go.AddComponent<CheckpointTimeAnalytics>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sessionId = Guid.NewGuid().ToString("N");

        BuildLookup();
        SceneManager.sceneLoaded += OnSceneLoaded;

        BeginLevel(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BeginLevel(scene.name);
    }

    private void BuildLookup()
    {
        checkpointCountByLevel.Clear();

        foreach (var cfg in levelConfigs)
        {
            if (cfg == null || string.IsNullOrWhiteSpace(cfg.levelName))
                continue;

            checkpointCountByLevel[cfg.levelName.Trim()] = Mathf.Max(1, cfg.checkpointCount);
        }
    }

    private bool IsTrackableScene(string levelName)
    {
        if (string.IsNullOrWhiteSpace(levelName))
            return false;

        if (checkpointCountByLevel.ContainsKey(levelName.Trim()))
            return true;

        string lower = levelName.Trim().ToLowerInvariant();
        return lower.Contains("tutorial") || lower.Contains("alpha");
    }

    private int ResolveCheckpointCount(string levelName)
    {
        if (checkpointCountByLevel.TryGetValue(levelName, out int count))
            return count;

        if (!string.IsNullOrWhiteSpace(levelName) &&
            levelName.IndexOf("tutorial", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return 3;
        }

        return 4;
    }

    private void BeginLevel(string levelName)
    {
        currentLevel = string.IsNullOrWhiteSpace(levelName) ? "Unknown" : levelName;

        if (!IsTrackableScene(currentLevel))
        {
            trackingActive = false;
            return;
        }

        expectedCheckpointCount = ResolveCheckpointCount(currentLevel);

        currentCheckpointNumber = 1;
        segmentStartTime = Time.time;
        trackingActive = true;

        if (verboseLogging)
        {
            Debug.Log($"[CheckpointTimeAnalytics] Begin level '{currentLevel}' with {expectedCheckpointCount} checkpoints.");
        }
    }

    public void RecordCheckpointReached(string checkpointName = "")
    {
        if (!trackingActive)
            BeginLevel(SceneManager.GetActiveScene().name);

        EmitCurrentSection(
            reason: "checkpoint_reached",
            checkpointName: string.IsNullOrWhiteSpace(checkpointName) ? $"Checkpoint {currentCheckpointNumber}" : checkpointName
        );

        if (currentCheckpointNumber >= expectedCheckpointCount)
        {
            trackingActive = false;

            if (verboseLogging)
            {
                Debug.Log($"[CheckpointTimeAnalytics] Final checkpoint reached in '{currentLevel}'.");
            }

            return;
        }

        currentCheckpointNumber++;
        segmentStartTime = Time.time;
    }

    public void CompleteLevel(string reason = "level_complete")
    {
        if (!trackingActive)
            return;

        EmitCurrentSection(
            reason: reason,
            checkpointName: $"Checkpoint {currentCheckpointNumber}"
        );

        trackingActive = false;
    }

    private void EmitCurrentSection(string reason, string checkpointName)
    {
        float endTime = Time.time;
        float duration = Mathf.Max(0f, endTime - segmentStartTime);

        CheckpointTimeEvent payload = new CheckpointTimeEvent
        {
            sessionId = sessionId,
            level = currentLevel,
            checkpointNumber = currentCheckpointNumber,
            checkpointName = checkpointName,
            startTime = segmentStartTime,
            endTime = endTime,
            durationSeconds = duration,
            reason = reason
        };

        string json = JsonUtility.ToJson(payload);

        if (gfAnalytics == null)
            gfAnalytics = FindObjectOfType<GoogleFormAnalytics>();

        if (gfAnalytics != null)
        {
            gfAnalytics.LogEvent("CheckpointTime/Segment", json);
        }
        else
        {
            Debug.LogWarning($"[CheckpointTimeAnalytics] GoogleFormAnalytics not found. Dropping: {json}");
        }

        if (verboseLogging)
        {
            Debug.Log($"[CheckpointTimeAnalytics] {currentLevel} / {checkpointName} = {duration:F2}s");
        }
    }
}