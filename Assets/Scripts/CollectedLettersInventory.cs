using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CollectedLettersInventory : MonoBehaviour
{
    public static CollectedLettersInventory Instance { get; private set; }

    [Header("Word Setup")]
    [Tooltip("Hidden target word for this level.")]
    public string targetWord = "FIRE";

    [Tooltip("Scene to load when the word is fully collected if no checkpoint controller handles the transition.")]
    public string wordFormationSceneName = "WordFormationScene";

    [Header("UI - HUD")]
    [Tooltip("Prefab for one top-row letter slot. Should be a UI circle with an Image and TMP text.")]
    public GameObject letterSlotPrefab;

    [Tooltip("Parent object that holds the top row of circles.")]
    public Transform lettersContainer;

    [Header("Colors")]
    public Color emptySlotColor = new Color(0.22f, 0.22f, 0.22f, 1f);
    public Color correctSlotColor = new Color(0.33f, 0.65f, 0.31f, 1f);
    public Color presentSlotColor = new Color(0.82f, 0.72f, 0.25f, 1f);
    public Color absentSlotColor = new Color(0.42f, 0.42f, 0.42f, 1f);
    public Color textColor = Color.white;

    private readonly List<GameObject> slotObjects = new List<GameObject>();

    public char?[] placedLetters;
    public string finalWord = "";

    private bool checkpoint5Triggered = false;

    public bool IsComplete => placedLetters != null && GetFirstEmptySlotIndex() < 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.SyncLevelDataFrom(this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        BuildEmptySlots();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Call this from a level setup script after the scene loads if you need to swap
    /// the target word or rebind scene-specific UI references.
    /// </summary>
    public void ConfigureLevel(
        string newTargetWord,
        Transform newLettersContainer,
        GameObject newLetterSlotPrefab,
        string newWordFormationSceneName = null)
    {
        if (!string.IsNullOrWhiteSpace(newTargetWord))
            targetWord = newTargetWord;

        if (newLettersContainer != null)
            lettersContainer = newLettersContainer;

        if (newLetterSlotPrefab != null)
            letterSlotPrefab = newLetterSlotPrefab;

        if (!string.IsNullOrWhiteSpace(newWordFormationSceneName))
            wordFormationSceneName = newWordFormationSceneName;

        BuildEmptySlots();
    }

    private void SyncLevelDataFrom(CollectedLettersInventory source)
    {
        if (source == null)
            return;

        if (!string.IsNullOrWhiteSpace(source.targetWord))
            targetWord = source.targetWord;

        if (source.lettersContainer != null)
            lettersContainer = source.lettersContainer;

        if (source.letterSlotPrefab != null)
            letterSlotPrefab = source.letterSlotPrefab;

        if (!string.IsNullOrWhiteSpace(source.wordFormationSceneName))
            wordFormationSceneName = source.wordFormationSceneName;

        BuildEmptySlots();
    }

    private void BuildEmptySlots()
    {
        if (lettersContainer == null || letterSlotPrefab == null)
        {
            Debug.LogWarning("CollectedLettersInventory: Missing lettersContainer or letterSlotPrefab.");
            return;
        }

        string normalizedTarget = GetNormalizedTargetWord();
        if (string.IsNullOrEmpty(normalizedTarget))
        {
            Debug.LogWarning("CollectedLettersInventory: targetWord is empty.");
            return;
        }

        for (int i = lettersContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(lettersContainer.GetChild(i).gameObject);
        }

        slotObjects.Clear();
        placedLetters = new char?[normalizedTarget.Length];
        finalWord = "";
        checkpoint5Triggered = false;

        for (int i = 0; i < normalizedTarget.Length; i++)
        {
            GameObject slot = Instantiate(letterSlotPrefab, lettersContainer, false);
            slot.name = $"LetterSlot_{i + 1}";
            slotObjects.Add(slot);

            Image bg = slot.GetComponent<Image>();
            if (bg == null)
                bg = slot.GetComponentInChildren<Image>();

            TMP_Text text = slot.GetComponentInChildren<TMP_Text>();

            if (bg != null)
                bg.color = emptySlotColor;

            if (text != null)
            {
                text.text = "";
                text.color = textColor;
            }
        }
    }

    public void AddLetter(char letter, int checkpointIndex = -1)
    {
        string normalizedTarget = GetNormalizedTargetWord();
        if (string.IsNullOrEmpty(normalizedTarget) || placedLetters == null || slotObjects.Count == 0)
            return;

        letter = char.ToUpperInvariant(letter);

        if (checkpointIndex >= 0 && checkpointIndex < slotObjects.Count)
        {
            PlaceLetterAtIndex(checkpointIndex, letter, normalizedTarget);
        }
        else
        {
            int firstEmpty = GetFirstEmptySlotIndex();
            if (firstEmpty >= 0)
            {
                PlaceLetterAtIndex(firstEmpty, letter, normalizedTarget);
            }
        }

        if (IsComplete && !checkpoint5Triggered)
        {
            checkpoint5Triggered = true;
            finalWord = GetCurrentWord();

            Debug.Log("Collected full word: " + finalWord);

            Checkpoint5Controller controller = FindFirstObjectByType<Checkpoint5Controller>();
            if (controller != null)
            {
                controller.EnterCheckpoint5();
            }
            else if (!string.IsNullOrWhiteSpace(wordFormationSceneName))
            {
                SceneManager.LoadScene(wordFormationSceneName);
            }
            else
            {
                Debug.LogError("Checkpoint5Controller not found and wordFormationSceneName is empty.");
            }
        }
    }

    private void PlaceLetterAtIndex(int index, char letter, string normalizedTarget)
    {
        if (index < 0 || index >= slotObjects.Count)
            return;

        GameObject slot = slotObjects[index];
        if (slot == null)
            return;

        placedLetters[index] = letter;

        Image bg = slot.GetComponent<Image>();
        if (bg == null)
            bg = slot.GetComponentInChildren<Image>();

        TMP_Text text = slot.GetComponentInChildren<TMP_Text>();

        if (text != null)
        {
            text.text = letter.ToString();
            text.color = textColor;
        }

        if (bg != null)
        {
            bool isCorrectPosition = index < normalizedTarget.Length && letter == normalizedTarget[index];
            bool existsElsewhere = normalizedTarget.Contains(letter.ToString());

            if (isCorrectPosition)
                bg.color = correctSlotColor;
            else if (existsElsewhere)
                bg.color = presentSlotColor;
            else
                bg.color = absentSlotColor;
        }
    }

    private int GetFirstEmptySlotIndex()
    {
        if (placedLetters == null)
            return -1;

        for (int i = 0; i < placedLetters.Length; i++)
        {
            if (!placedLetters[i].HasValue)
                return i;
        }

        return -1;
    }

    public string GetCurrentWord()
    {
        if (placedLetters == null)
            return "";

        char[] chars = new char[placedLetters.Length];
        for (int i = 0; i < placedLetters.Length; i++)
        {
            chars[i] = placedLetters[i].HasValue ? placedLetters[i].Value : '_';
        }

        finalWord = new string(chars);
        return finalWord;
    }

    private string GetNormalizedTargetWord()
    {
        return string.IsNullOrWhiteSpace(targetWord) ? "" : targetWord.Trim().ToUpperInvariant();
    }

    public void ResetInventory()
    {
        BuildEmptySlots();
    }

    public char[] GetCollectedLettersInOrder()
    {
        List<char> letters = new List<char>();

        if (placedLetters == null)
            return letters.ToArray();

        for (int i = 0; i < placedLetters.Length; i++)
        {
            if (placedLetters[i].HasValue)
                letters.Add(char.ToUpperInvariant(placedLetters[i].Value));
        }

        return letters.ToArray();
    }
}