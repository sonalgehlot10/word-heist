using UnityEngine;

public class BridgePowerup : MonoBehaviour
{
    public BridgePlatform bridgeToActivate;

    [Tooltip("Hint to show when player collects this powerup (e.g. 'Press B to Activate Bridge')")]
    public GameObject collectHint;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerBridgeAbility player = other.GetComponent<PlayerBridgeAbility>();

            if (player != null)
            {
                player.CollectBridge(bridgeToActivate);
            }

            if (collectHint != null)
                collectHint.SetActive(true);

            Destroy(gameObject);
        }
    }
}