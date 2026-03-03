using UnityEngine;

// Attach this to the SAME object that has the TRIGGER collider (TrampolineTrigger).
public class TrampolineLauncher2D : MonoBehaviour
{
    [Header("Direction Meter")]
    public float minAngleDeg = 30f;
    public float maxAngleDeg = 150f;
    public float oscillationSpeed = 0.6f;

    [Header("Launch")]
    public float launchSpeed = 18f;
    public float movementLockDuration = 0.20f;

    [Tooltip("Minimum time between trampoline launches.")]
    public float launchCooldown = 0.35f;

    [Tooltip("Require player to be grounded (standing) to launch.")]
    public bool requireGrounded = true;

    [Header("Needle Visual")]
    public LineRenderer needleLine;
    public float needleLength = 0.8f;
    public Vector2 needleLocalOffset = new Vector2(0f, 1.0f);

    private Rigidbody2D playerRbInside;
    private PlayerMovement playerMovementInside;
    private float currentAngleDeg;

    private float nextAllowedLaunchTime = 0f;

    void Update()
    {
        // 1) Animate needle
        float t = Mathf.PingPong(Time.time * oscillationSpeed, 1f);
        currentAngleDeg = Mathf.Lerp(minAngleDeg, maxAngleDeg, t);
        UpdateNeedleVisual();

        // 2) If no player, do nothing
        if (playerRbInside == null) return;

        // 3) Space press -> attempt launch (with cooldown + grounded check)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryLaunch();
        }
    }

    private void TryLaunch()
    {
        // cooldown
        if (Time.time < nextAllowedLaunchTime) return;

        // require grounded (prevents mid-air spam)
        if (requireGrounded && playerMovementInside != null)
        {
            // PlayerMovement has isGrounded private, so we use a safe approximation:
            // If player's vertical speed is not near zero, treat as not grounded.
            // (If you want a perfect check, I can expose isGrounded as a public getter.)
            if (Mathf.Abs(playerRbInside.linearVelocity.y) > 0.05f)
            {
                return;
            }
        }

        // compute direction
        float rad = currentAngleDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

        Vector2 launchVel = dir * launchSpeed;

        // apply via PlayerMovement override (prevents FixedUpdate X overwrite)
        if (playerMovementInside != null)
        {
            playerMovementInside.ApplyExternalLaunch(launchVel, movementLockDuration);
        }
        else
        {
            playerRbInside.linearVelocity = launchVel;
        }

        nextAllowedLaunchTime = Time.time + launchCooldown;
    }

    private void UpdateNeedleVisual()
    {
        if (needleLine == null) return;

        float rad = currentAngleDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

        Vector3 start = new Vector3(needleLocalOffset.x, needleLocalOffset.y, 0f);
        Vector3 end = start + (Vector3)(dir * needleLength);

        needleLine.positionCount = 2;
        needleLine.SetPosition(0, start);
        needleLine.SetPosition(1, end);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerRbInside = other.attachedRigidbody;
        playerMovementInside = other.GetComponent<PlayerMovement>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerRbInside = null;
        playerMovementInside = null;
    }
}