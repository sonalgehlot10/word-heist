using UnityEngine;

public class BridgePlatform : MonoBehaviour
{
    Collider2D col;
    public GameObject hint;

    void Start()
    {
        col = GetComponent<Collider2D>();
        col.enabled = false; // bridge starts passable
    }

    public void ActivateBridge()
    {
        Debug.Log("Bridge Activated!");
        col.enabled = true;

        if (hint != null)
        {
            hint.SetActive(false);
        }
    }
}