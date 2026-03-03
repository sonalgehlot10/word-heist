using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CheckpointPortal : MonoBehaviour
{
    // Hardcoded 2D entry position for the next stage. Set this in Inspector per-prefab.
    public Vector2 entryPosition;

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Robust: find PlayerMovement on the triggered object or its parents.
        PlayerMovement pm = other.GetComponentInParent<PlayerMovement>();
        if (pm == null) return;

        // Teleport player to next-stage entry and update player's respawn to this entry.
        pm.TeleportToStage(entryPosition);
    }
}