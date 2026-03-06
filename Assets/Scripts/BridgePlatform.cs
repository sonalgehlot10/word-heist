using UnityEngine;

public class BridgePlatform : MonoBehaviour
{
    Collider2D col;
    SpriteRenderer sr;
    public GameObject hint;

    void Start()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        col.enabled = false; // bridge starts passable

        if (sr != null)
        {
            sr.color = new Color32(75,75,75,255);
        }
    }

    public void ActivateBridge()
    {
        // Debug.Log("Bridge Activated!");
        Debug.Log("Activated " + gameObject.name);
        col.enabled = true;

        if (sr != null)
        {
            sr.color = new Color32(255,255,255,255); // bridge becomes white (#FFFFFF)
        }

        if (hint != null)
        {
            hint.SetActive(false);
        }
    }
}