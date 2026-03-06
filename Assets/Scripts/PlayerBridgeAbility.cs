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