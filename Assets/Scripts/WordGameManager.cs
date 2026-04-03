using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WordGameManager : MonoBehaviour
{
    public static WordGameManager Instance;

    public Transform slotsParent;
    public GameObject slotPrefab;
    public TMP_Text timerText;

    public string targetWord = "FIRE";
    public float timeRemaining = 10f;
    private readonly List<WordSlot> slots = new List<WordSlot>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        string word = targetWord;
        if (CollectedLettersInventory.Instance != null && !string.IsNullOrWhiteSpace(CollectedLettersInventory.Instance.finalWord))
            word = CollectedLettersInventory.Instance.finalWord;

        if (slotsParent != null && slotPrefab != null && !string.IsNullOrEmpty(word))
        {
            foreach (char c in word)
            {
                GameObject go = Instantiate(slotPrefab, slotsParent);
                WordSlot slot = go.GetComponent<WordSlot>();
                if (slot != null)
                {
                    slot.Setup(c);
                    slots.Add(slot);
                }
            }
        }

        CheckWord();
        StartCoroutine(Timer());
    }

    private IEnumerator Timer()
    {
        while (timeRemaining > 0)
        {
            if (timerText != null)
                timerText.text = Mathf.CeilToInt(timeRemaining).ToString();

            timeRemaining -= Time.deltaTime;
            yield return null;

            if (IsCorrect())
            {
                SceneManager.LoadScene("VictoryScene");
                yield break;
            }
        }

        SceneManager.LoadScene("RetryScene");
    }

    public void CheckWord()
    {
        if (slots.Count == 0)
            return;

        for (int i = 0; i < slots.Count && i < targetWord.Length; i++)
        {
            if (slots[i].GetLetter().ToString() == targetWord[i].ToString())
                slots[i].SetGreen();
            else
                slots[i].SetYellow();
        }
    }

    private bool IsCorrect()
    {
        if (slots.Count == 0 || slots.Count != targetWord.Length)
            return false;

        string current = "";
        foreach (WordSlot s in slots)
            current += s.GetLetter();

        return current == targetWord;
    }
}
