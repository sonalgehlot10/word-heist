using TMPro;
using UnityEngine;

public class LetterDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text letterText;
    [SerializeField] private char letter = 'A';

    void Start()
    {
        UpdateLetter();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        UpdateLetter();
    }
#endif

    void UpdateLetter()
    {
        if (letterText != null)
            letterText.text = char.ToUpper(letter).ToString();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Collected: " + letter);

            Destroy(gameObject);
        }
    }
}