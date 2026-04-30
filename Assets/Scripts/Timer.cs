using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] private float totalTime = 5f;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject timerRoot;

    [Header("Controller")]
    [SerializeField] private Checkpoint5Controller controller;

    private float currentTime;
    private bool running;
    private bool finished;

    private void Start()
    {
        if (timerRoot != null)
            timerRoot.SetActive(false);
    }

    private void Update()
    {
        if (!running || finished)
            return;

        currentTime -= Time.deltaTime;

        if (timerText != null)
            timerText.text = "Time Left: " + Mathf.CeilToInt(currentTime).ToString();

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            running = false;
            finished = true;

            if (controller != null)
                controller.ShowLoseText();
        }
    }

    public void StartTimer()
    {
        currentTime = totalTime;
        running = true;
        finished = false;

        if (timerRoot != null)
            timerRoot.SetActive(true);
    }

    public void StopTimer()
    {
        running = false;
    }
}