using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public Vector2 respawnPosition;
    private GoogleFormAnalytics gfAnalytics;

    private bool levelComplete = false;
    public GameObject winText;
    public string ExitSceneName = "ExitScene";

    private bool externalLaunchActive = false;
    private float externalLaunchEndTime = 0f;

    [Header("Zipline")]
    public float ziplineSpeed = 6f;
    public float ziplineEndThreshold = 0.15f;
    private float lastZiplineExitTime = -1f;
    private float ziplineCooldown = 0.3f;

    private Zipline ziplineInRange;
    private bool isOnZipline = false;
    private Vector2 ziplineDirection;
    private Vector2 ziplineTarget;
    private float normalGravity;

    void Awake()
    {
        gfAnalytics = FindObjectOfType<GoogleFormAnalytics>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        normalGravity = rb.gravityScale;

        if (respawnPosition == Vector2.zero)
            respawnPosition = new Vector2(transform.position.x, transform.position.y);
    }

    public void ApplyExternalLaunch(Vector2 newVelocity, float lockDuration)
    {
        rb.linearVelocity = newVelocity;
        externalLaunchActive = true;
        externalLaunchEndTime = Time.time + lockDuration;
    }

    void Update()
    {
        if (levelComplete) return;

        if (isOnZipline)
        {
            Vector2 currentPos = transform.position;
            Vector2 nextPos = currentPos + ziplineDirection * ziplineSpeed * Time.deltaTime;

            Vector2 toTarget = ziplineTarget - currentPos;

            if (Vector2.Dot(toTarget, ziplineDirection) <= 0f ||
                Vector2.Distance(currentPos, ziplineTarget) < 0.1f)
            {
                transform.position = ziplineTarget;
                StopZipline();
            }
            else
            {
                transform.position = nextPos;
            }

            return;
        }

        moveInput = Input.GetAxisRaw("Horizontal");

        if (!externalLaunchActive && Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void FixedUpdate()
    {
        if (levelComplete) return;

        if (externalLaunchActive)
        {
            if (Time.time >= externalLaunchEndTime)
            {
                externalLaunchActive = false;
            }
            else
            {
                return;
            }
        }

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    void LateUpdate()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }
    }

    public void TeleportToStage(Vector2 entryPosition2D)
    {
        Vector3 target = new Vector3(entryPosition2D.x, entryPosition2D.y, transform.position.z);

        transform.position = target;
        if (rb != null)
        {
            rb.position = target;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        respawnPosition = entryPosition2D;
    }

    void Respawn()
    {
        Vector3 newPos = new Vector3(respawnPosition.x, respawnPosition.y, transform.position.z);

        transform.position = newPos;
        if (rb != null)
        {
            rb.position = newPos;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    // ✅ FIXED FUNCTION (only change made)
    private void StartZipline()
    {
        if (Time.time - lastZiplineExitTime < ziplineCooldown)
            return;
        if (ziplineInRange == null) return;

        Vector2 start = ziplineInRange.GetStart();
        Vector2 end = ziplineInRange.GetEnd();

        float distToStart = Vector2.Distance(transform.position, start);
        float distToEnd = Vector2.Distance(transform.position, end);

        isOnZipline = true;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        Vector2 lineDir = (end - start).normalized;
        Vector2 toPlayer = (Vector2)transform.position - start;

        float projection = Vector2.Dot(toPlayer, lineDir);
        projection = Mathf.Clamp(projection, 0f, Vector2.Distance(start, end));

        Vector2 closestPoint = start + lineDir * projection;
        transform.position = closestPoint;

        // ❌ Removed position snapping
        // transform.position = start/end

        if (distToStart <= distToEnd)
        {
            ziplineDirection = (end - start).normalized;
            ziplineTarget = end;
        }
        else
        {
            ziplineDirection = (start - end).normalized;
            ziplineTarget = start;
        }
    }

    private void StopZipline()
    {
        isOnZipline = false;
        rb.gravityScale = normalGravity;
        rb.linearVelocity = Vector2.zero;
        ziplineInRange = null;
        lastZiplineExitTime = Time.time;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isOnZipline) return;

        Zipline zip = other.GetComponentInParent<Zipline>();
        if (zip != null)
        {
            ziplineInRange = zip;
            StartZipline();
        }

        if (other.CompareTag("Spike"))
        {
            gfAnalytics.LogEvent("Death/Respawn", "Hit Spike");
            CheckpointTracker.Instance?.LogAttempt();
            Respawn();
            return;
        }

        if (other.CompareTag("Exit"))
        {
            Debug.Log("EXIT TRIGGERED");

            AnalyticsManager am = AnalyticsManager.Instance;
            EnergyBar energyBar = FindObjectOfType<EnergyBar>();

            if (am != null && energyBar != null)
            {
                int reboosts = am.GetReboostCount();
                float stamina = energyBar.GetStaminaPercent();

                Debug.Log($"ANALYTIC → Reboosts: {reboosts}, Final Stamina: {stamina}");

                gfAnalytics.LogEvent(
                    "Reboost_vs_Stamina",
                    $"Reboosts:{reboosts}, FinalStamina:{stamina}"
                );

                am.ResetLevelAnalytics();
            }

            LetterInventory li = FindObjectOfType<LetterInventory>();
            if (li != null)
            {
                if (GameState.Instance == null)
                {
                    var go = new GameObject("GameState");
                    go.AddComponent<GameState>();
                }
                GameState.Instance.collectedLettersSnapshot = li.GetCollectedLetters();
            }

            CheckpointTimeAnalytics.Instance?.CompleteLevel("level_complete");

            SceneManager.LoadScene(ExitSceneName);
            return;
        }


        // compatibility: if a plain checkpoint object with tag "Checkpoint" exists,
        // use its transform position as respawn (unchanged behavior)
        if (other.CompareTag("Checkpoint"))
        {
            respawnPosition = new Vector2(other.transform.position.x, other.transform.position.y);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Zipline zip = other.GetComponentInParent<Zipline>();
        if (zip != null && ziplineInRange == zip && !isOnZipline)
        {
            ziplineInRange = null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Guard"))
        {
            if (PlayerFreezeAbility.FreezeActive)
                return;

            gfAnalytics.LogEvent("Death/Respawn", "Caught by Guard");
            CheckpointTracker.Instance?.LogAttempt();
            Respawn();
        }
    }
}