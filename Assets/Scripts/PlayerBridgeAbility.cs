// using UnityEngine;

// public class PlayerBridgeAbility : MonoBehaviour
// {
//     public GameObject bridgeIcon;
//     public GameObject bridge;

//     private bool hasBridge = false;

//     public void CollectBridge()
//     {
//         hasBridge = true;

//         if (bridgeIcon != null)
//         {
//             bridgeIcon.SetActive(true);
//         }
//     }

//     void Update()
//     {
//         if (hasBridge && Input.GetKeyDown(KeyCode.B))
//         {
//             ActivateBridge();
//         }
//     }

//     void ActivateBridge()
//     {
//         Collider2D col = bridge.GetComponent<Collider2D>();

//         col.isTrigger = false;

//         bridgeIcon.SetActive(false);

//         hasBridge = false;
//     }
// }

using UnityEngine;

public class PlayerBridgeAbility : MonoBehaviour
{
    bool hasBridge = false;
    BridgePlatform targetBridge;
    public GameObject bridgeIcon;

    void Update()
    {
        if (hasBridge && Input.GetKeyDown(KeyCode.B))
        {
            if (targetBridge != null)
                targetBridge.ActivateBridge();

            hasBridge = false;
            targetBridge = null;

            if (bridgeIcon != null)
                bridgeIcon.SetActive(false);
        }
    }

    public void CollectBridge(BridgePlatform bridge)
    {
        hasBridge = true;
        targetBridge = bridge;

        if (bridgeIcon != null)
            bridgeIcon.SetActive(true);
    }
}