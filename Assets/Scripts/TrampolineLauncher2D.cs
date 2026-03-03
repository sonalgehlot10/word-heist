using UnityEngine;

// Attach this to the SAME object that has the TRIGGER collider (TrampolineTrigger).
public class TrampolineLauncher2D : MonoBehaviour
{
    [Header("Center Snap")]
    public bool snapPlayerToCenter = true;

    [Tooltip("How fast to pull player to center (higher = faster).")]
    public float snapSpeed = 25f;

    [Tooltip("Stop snapping when player is this close to center (in units).")]
    public float snapDeadZone = 0.02f;

    [Tooltip("Optional: freeze horizontal velocity while snapping.")]
    public bool zeroPlayerXVelocityWhileSnapping = true;

    [Tooltip("If empty, we will auto-use the Collider2D on this object.")]
    public Collider2D triggerCollider;

    [Header("Direction Meter")]
    public float minAngleDeg = 30f;
    public float maxAngleDeg = 150f;
    public float oscillationSpeed = 0.6f;

    [Header("Launch")]
    public float launchSpeed = 14f;
    public float movementLockDuration = 0.20f;
    public float launchCooldown = 0.35f;

    [Header("Needle Visual")]
    public LineRenderer needleLine;
    public float needleLength = 0.8f;
    public Vector2 needleLocalOffset = new Vector2(0f, 1.0f);

    private Rigidbody2D playerRbInside;
    private PlayerMovement playerMovementInside;

    private float currentAngleDeg;
    private float nextAllowedLaunchTime = 0f;

    void Awake()
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider2D>();
        }
    }

    private float GetCenterX()
    {
        if (triggerCollider != null)
        {
            return triggerCollider.bounds.center.x;
        }

        return transform.position.x;
    }

    void Update()
    {
        // Needle animation
        float t = Mathf.PingPong(Time.time * oscillationSpeed, 1f);
        currentAngleDeg = Mathf.Lerp(minAngleDeg, maxAngleDeg, t);
        UpdateNeedleVisual();

        if (playerRbInside == null) return;

        // Launch on Space (cooldown)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryLaunch();
        }
    }

    void FixedUpdate()
    {
        if (!snapPlayerToCenter) return;
        if (playerRbInside == null) return;

        float centerX = GetCenterX();

        // Pull player to center X smoothly
        float px = playerRbInside.position.x;
        float dx = centerX - px;

        if (Mathf.Abs(dx) <= snapDeadZone) return;

        float newX = Mathf.MoveTowards(px, centerX, snapSpeed * Time.fixedDeltaTime);
        Vector2 newPos = new Vector2(newX, playerRbInside.position.y);
        playerRbInside.MovePosition(newPos);

        if (zeroPlayerXVelocityWhileSnapping)
        {
            playerRbInside.linearVelocity = new Vector2(0f, playerRbInside.linearVelocity.y);
        }
    }

    private void TryLaunch()
    {
        if (Time.time < nextAllowedLaunchTime) return;

        float rad = currentAngleDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
        Vector2 launchVel = dir * launchSpeed;

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