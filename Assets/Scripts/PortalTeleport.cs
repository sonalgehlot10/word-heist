using UnityEngine;

public class PortalTeleport : MonoBehaviour
{
    public Transform destination;

    private static bool isTeleporting = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (isTeleporting) return;

        isTeleporting = true;

        // other.transform.position = destination.position + new Vector3(0, 2f, 0);
        other.transform.position = destination.position;

        Invoke(nameof(ResetTeleport), 0.3f);
    }

    void ResetTeleport()
    {
        isTeleporting = false;
    }
}