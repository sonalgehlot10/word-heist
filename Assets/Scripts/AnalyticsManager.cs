using UnityEngine;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance;

    private int reboostCount = 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (Instance == null)
        {
            GameObject obj = new GameObject("AnalyticsManager");
            obj.transform.parent = null;
            obj.AddComponent<AnalyticsManager>();
        }
    }

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Persist across scenes
        DontDestroyOnLoad(gameObject);
    }

    public void IncrementReboost()
    {
        reboostCount++;
        Debug.Log("Reboost Count: " + reboostCount); // debug
    }

    public int GetReboostCount()
    {
        return reboostCount;
    }

    public void ResetLevelAnalytics()
    {
        reboostCount = 0;
    }
}