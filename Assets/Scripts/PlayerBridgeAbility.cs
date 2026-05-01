using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerBridgeAbility : MonoBehaviour
{
    [Header("Bridge Use")]
    [SerializeField] private KeyCode useKey = KeyCode.B;

    [Header("HUD")]
    [SerializeField] private TMP_Text bridgeCountText;

    private readonly Queue<BridgePlatform> bridgeQueue = new Queue<BridgePlatform>();

    private void Start()
    {
        RefreshBridgeUI();
    }

    private void Update()
    {
        if (bridgeQueue.Count > 0 && Input.GetKeyDown(useKey))
        {
            BridgePlatform targetBridge = bridgeQueue.Dequeue();

            if (targetBridge != null)
                targetBridge.ActivateBridge();

            RefreshBridgeUI();
        }
    }

    public void CollectBridge(BridgePlatform bridge)
    {
        if (bridge != null)
            bridgeQueue.Enqueue(bridge);

        RefreshBridgeUI();
    }

    private void RefreshBridgeUI()
    {
        if (bridgeCountText != null)
            bridgeCountText.text = bridgeQueue.Count.ToString();
    }
}