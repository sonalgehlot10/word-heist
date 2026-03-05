using UnityEngine;

public class PortalTraveler : MonoBehaviour
{
    [HideInInspector] public float nextPortalTime = 0f;

    public bool CanUsePortal(float cooldownSeconds)
    {
        return Time.time >= nextPortalTime;
    }

    public void SetPortalCooldown(float cooldownSeconds)
    {
        nextPortalTime = Time.time + cooldownSeconds;
    }
}