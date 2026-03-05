using UnityEngine;
using UnityEngine.UI;

public class EnergyMeter : MonoBehaviour
{
    [Header("UI")]
    public Slider energyBar; // drag your vertical Slider here

    [Header("Energy Settings")]
    public float maxEnergy = 100f;

    // Lower = fills slower. Start with ~1.0–1.5
    public float energyPerUnit = 0.2f;

    [Header("Movement Weights")]
    public float verticalWeight = 0.4f; // jump/ladder counts, but less than horizontal

    [Header("Filtering")]
    public float minMovementThreshold = 0.01f; // ignore tiny jitter
    public float axisDeadZone = 0.1f;

    private float energy = 0f;
    private Vector2 lastPos;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        lastPos = rb ? rb.position : (Vector2)transform.position;

        if (energyBar != null)
        {
            energyBar.minValue = 0f;
            energyBar.maxValue = 1f;
            energyBar.value = 0f;
        }
    }

    void LateUpdate()
    {
        Vector2 currentPos = rb ? rb.position : (Vector2)transform.position;

        // Movement (split X and Y so we can weight jump/ladder)
        float dx = Mathf.Abs(currentPos.x - lastPos.x);
        float dy = Mathf.Abs(currentPos.y - lastPos.y);
        float movement = dx + dy * verticalWeight;

        // Intent (axis + key fallback so left/right always works)
        float hAxis = Input.GetAxisRaw("Horizontal");
        float vAxis = Input.GetAxisRaw("Vertical");

        bool axisIntent = Mathf.Abs(hAxis) > axisDeadZone || Mathf.Abs(vAxis) > axisDeadZone;

        bool keyIntent =
            Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) ||
            Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow);

        bool jumpIntent = Input.GetKey(KeyCode.Space); // count jump as intent too

        bool hasIntent = axisIntent || keyIntent || jumpIntent;
        bool actuallyMoved = movement > minMovementThreshold;

        // Only charge when BOTH intent + actual movement happen
        if (hasIntent && actuallyMoved)
        {
            energy += movement * energyPerUnit;
            energy = Mathf.Clamp(energy, 0f, maxEnergy);

            if (energyBar != null)
                energyBar.value = energy / maxEnergy;

            if (energy >= maxEnergy)
                Debug.Log("Energy full! Game Over.");
        }

        lastPos = currentPos;
    }
}