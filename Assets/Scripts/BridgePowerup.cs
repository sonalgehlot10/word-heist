// using UnityEngine;

// public class BridgePowerup : MonoBehaviour
// {
//     void OnTriggerEnter2D(Collider2D other)
//     {
//         if (other.CompareTag("Player"))
//         {
//             PlayerBridgeAbility player = other.GetComponent<PlayerBridgeAbility>();

//             if (player != null)
//             {
//                 player.CollectBridge();
//             }

//             Debug.Log("Destroying powerup: " + gameObject.name);

//             Destroy(gameObject);
//         }
//     }
// }

using UnityEngine;

public class BridgePowerup : MonoBehaviour
{
    public BridgePlatform bridgeToActivate;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerBridgeAbility player = other.GetComponent<PlayerBridgeAbility>();

            if (player != null)
            {
                player.CollectBridge(bridgeToActivate);
            }

            Destroy(gameObject);
        }
    }
}