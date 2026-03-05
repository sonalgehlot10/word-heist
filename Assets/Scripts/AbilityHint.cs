using UnityEngine;

public class AbilityHint : MonoBehaviour
{
    public GameObject hintText;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            hintText.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            hintText.SetActive(false);
        }
    }
}