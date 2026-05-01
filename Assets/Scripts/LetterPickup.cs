using TMPro;
using UnityEngine;

public class LetterPickup : MonoBehaviour
{
    [SerializeField] private TMP_Text tmp;
    [SerializeField] private char letter = 'A';
    [SerializeField] private int checkpointIndex;

    private void Awake()
    {
        tmp = GetComponentInChildren<TMP_Text>(true);
    }

    public void SetLetter(char c)
    {
        letter = char.ToUpperInvariant(c);

        if (tmp == null)
            tmp = GetComponentInChildren<TMP_Text>(true);

        if (tmp != null)
            tmp.text = letter.ToString();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"PICKUP: '{letter}' int={(int)letter} obj={gameObject.name}");

        if (!other.CompareTag("Player"))
            return;

        if (CollectedLettersInventory.Instance != null)
            CollectedLettersInventory.Instance.AddLetter(letter, checkpointIndex);
        else
            Debug.LogError("CollectedLettersInventory.Instance is NULL!");

        Destroy(gameObject);
    }
}
