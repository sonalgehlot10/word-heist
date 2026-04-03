using TMPro;
using UnityEngine;

public class LetterDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text letterText;
    [SerializeField] private char letter = 'A';
    [SerializeField] private int checkpointIndex;

    private void Start()
    {
        if (letterText != null)
            letterText.text = letter.ToString();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log("Inventory instance: " + CollectedLettersInventory.Instance);

        if (CollectedLettersInventory.Instance != null)
        {
            CollectedLettersInventory.Instance.AddLetter(letter, checkpointIndex);
        }
        else
        {
            Debug.LogError("CollectedLettersInventory.Instance is NULL!");
        }

        Destroy(gameObject);
    }
}
