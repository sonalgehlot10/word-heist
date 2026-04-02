using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnergyBar : MonoBehaviour
{
    [Header("Legacy UI")]
    [Tooltip("Optional old slider UI. It will be hidden at runtime if assigned.")]
    public Slider energyBar;

    [Header("Energy Settings")]
    public float maxEnergy = 100f;
    public float drainPerUnit = 0.2f;
    public float verticalWeight = 0.4f;

    [Header("Filtering")]
    public float minMovementThreshold = 0.01f;
    public float axisDeadZone = 0.1f;

    [Header("Block Bar")]
    [Range(1, 12)] public int blockCount = 5;
    public Vector3 barLocalOffset = new Vector3(0f, 0.2f, 0f);
    public Vector2 blockSize = new Vector2(0.18f, 0.28f);
    public float blockSpacing = 0.05f;

    public Color healthyColor = new Color(0.20f, 0.85f, 0.25f, 1f);
    public Color warningColor = new Color(0.95f, 0.90f, 0.20f, 1f);
    public Color dangerColor = new Color(0.95f, 0.20f, 0.20f, 1f);

    [Tooltip("Color for empty blocks.")]
    public Color emptyColor = new Color(0.12f, 0.12f, 0.12f, 0.9f);

    [Header("Borders")]
    public Color activeBorderColor = new Color(1f, 1f, 1f, 0.10f);
    public Color emptyBorderColor = new Color(1f, 1f, 1f, 0.60f);

    [Tooltip("How much smaller the fill is compared to the border.")]
    public Vector2 fillInset = new Vector2(0.03f, 0.03f);

    public Color backgroundColor = new Color(0f, 0f, 0f, 0.35f);

    [Header("Warning Text")]
    public string warningMessage = "Energy draining!";
    public Vector3 warningTextOffset = new Vector3(0f, 0.42f, 0f);
    public int warningFontSize = 55;
    public float warningCharacterSize = 0.05f;
    public float flickerSpeed = 6f;

    private float energy;
    private Vector2 lastPos;
    private Rigidbody2D rb;
    private GoogleFormAnalytics gfAnalytics;

    private Transform blockBarRoot;
    private SpriteRenderer[] blockRenderers;   // inner fill
    private SpriteRenderer[] borderRenderers;   // outer border

    private Sprite whiteSprite;

    private TextMesh warningText;
    private float flickerTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        gfAnalytics = FindObjectOfType<GoogleFormAnalytics>();
    }

    void Start()
    {
        energy = maxEnergy;
        lastPos = rb ? rb.position : (Vector2)transform.position;

        if (energyBar != null)
        {
            energyBar.gameObject.SetActive(false);
        }

        BuildBlockBar();
        RefreshBlockBar();
    }

    void LateUpdate()
    {
        Vector2 currentPos = rb ? rb.position : (Vector2)transform.position;

        float dx = Mathf.Abs(currentPos.x - lastPos.x);
        float dy = Mathf.Abs(currentPos.y - lastPos.y);
        float movement = dx + dy * verticalWeight;

        float hAxis = Input.GetAxisRaw("Horizontal");
        float vAxis = Input.GetAxisRaw("Vertical");

        bool axisIntent = Mathf.Abs(hAxis) > axisDeadZone || Mathf.Abs(vAxis) > axisDeadZone;

        bool keyIntent =
            Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) ||
            Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow);

        bool jumpIntent = Input.GetKey(KeyCode.Space);

        bool hasIntent = axisIntent || keyIntent || jumpIntent;
        bool actuallyMoved = movement > minMovementThreshold;

        if (hasIntent && actuallyMoved)
        {
            energy -= movement * drainPerUnit;
            energy = Mathf.Clamp(energy, 0f, maxEnergy);

            RefreshBlockBar();

            if (energy <= 0f)
                GameOver();
        }

        lastPos = currentPos;

        if (blockBarRoot != null)
        {
            blockBarRoot.localPosition = barLocalOffset;
        }

        UpdateWarningFlicker();
    }

    private void BuildBlockBar()
    {
        if (blockBarRoot != null)
            return;

        blockBarRoot = new GameObject("EnergyBlockBar").transform;
        blockBarRoot.SetParent(transform, false);
        blockBarRoot.localPosition = barLocalOffset;
        blockBarRoot.localRotation = Quaternion.identity;
        blockBarRoot.localScale = Vector3.one;

        whiteSprite = CreateWhiteSprite();

        // Background behind the blocks
        GameObject background = new GameObject("Background");
        background.transform.SetParent(blockBarRoot, false);
        background.transform.localPosition = Vector3.zero;
        background.transform.localScale = new Vector3(
            (blockCount * blockSize.x) + ((blockCount - 1) * blockSpacing) + 0.12f,
            blockSize.y + 0.10f,
            1f
        );

        var bgRenderer = background.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = whiteSprite;
        bgRenderer.color = backgroundColor;
        bgRenderer.sortingOrder = 0;

        blockRenderers = new SpriteRenderer[blockCount];
        borderRenderers = new SpriteRenderer[blockCount];

        float totalWidth = (blockCount * blockSize.x) + ((blockCount - 1) * blockSpacing);
        float startX = -totalWidth * 0.5f + (blockSize.x * 0.5f);

        for (int i = 0; i < blockCount; i++)
        {
            GameObject blockRoot = new GameObject($"Block_{i + 1}");
            blockRoot.transform.SetParent(blockBarRoot, false);
            blockRoot.transform.localPosition = new Vector3(startX + i * (blockSize.x + blockSpacing), 0f, 0f);
            blockRoot.transform.localRotation = Quaternion.identity;
            blockRoot.transform.localScale = Vector3.one;

            // Border: same overall size for every block
            GameObject border = new GameObject("Border");
            border.transform.SetParent(blockRoot.transform, false);
            border.transform.localPosition = Vector3.zero;
            border.transform.localScale = new Vector3(blockSize.x, blockSize.y, 1f);

            var borderRenderer = border.AddComponent<SpriteRenderer>();
            borderRenderer.sprite = whiteSprite;
            borderRenderer.color = emptyBorderColor;
            borderRenderer.sortingOrder = 1;
            borderRenderers[i] = borderRenderer;

            // Fill: slightly smaller so the border is visible, but all blocks keep the same outer size
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(blockRoot.transform, false);
            fill.transform.localPosition = Vector3.zero;
            fill.transform.localScale = new Vector3(
                Mathf.Max(0.01f, blockSize.x - fillInset.x),
                Mathf.Max(0.01f, blockSize.y - fillInset.y),
                1f
            );

            var fillRenderer = fill.AddComponent<SpriteRenderer>();
            fillRenderer.sprite = whiteSprite;
            fillRenderer.color = healthyColor;
            fillRenderer.sortingOrder = 2;
            blockRenderers[i] = fillRenderer;
        }

        GameObject textObj = new GameObject("EnergyWarningText");
        textObj.transform.SetParent(blockBarRoot, false);
        textObj.transform.localPosition = warningTextOffset;
        textObj.transform.localRotation = Quaternion.identity;
        textObj.transform.localScale = Vector3.one;

        warningText = textObj.AddComponent<TextMesh>();
        warningText.text = warningMessage;
        warningText.fontSize = warningFontSize;
        warningText.characterSize = warningCharacterSize;
        warningText.alignment = TextAlignment.Center;
        warningText.anchor = TextAnchor.LowerCenter;
        warningText.color = new Color(1f, 0.2f, 0.2f, 0f);

        var textRenderer = textObj.GetComponent<MeshRenderer>();
        if (textRenderer != null)
        {
            textRenderer.sortingOrder = 3;
        }

        warningText.gameObject.SetActive(false);
    }

    private Sprite CreateWhiteSprite()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        texture.hideFlags = HideFlags.HideAndDontSave;
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    private void RefreshBlockBar()
    {
        if (blockRenderers == null || blockRenderers.Length == 0)
            return;

        float energyPerBlock = maxEnergy / blockCount;
        int activeBlocks = Mathf.CeilToInt(energy / energyPerBlock);
        activeBlocks = Mathf.Clamp(activeBlocks, 0, blockCount);

        bool isCritical = activeBlocks <= 1;

        Color activeColor;
        if (activeBlocks <= 1)
            activeColor = dangerColor;
        else if (activeBlocks <= 3)
            activeColor = warningColor;
        else
            activeColor = healthyColor;

        for (int i = 0; i < blockRenderers.Length; i++)
        {
            bool active = i < activeBlocks;

            if (blockRenderers[i] != null)
            {
                blockRenderers[i].color = active ? activeColor : emptyColor;
                blockRenderers[i].enabled = true;
            }

            if (borderRenderers != null && i < borderRenderers.Length && borderRenderers[i] != null)
            {
                borderRenderers[i].color = active ? activeBorderColor : emptyBorderColor;
                borderRenderers[i].enabled = true;
            }
        }

        if (warningText != null)
        {
            warningText.gameObject.SetActive(isCritical);
            if (!isCritical)
            {
                Color c = warningText.color;
                c.a = 0f;
                warningText.color = c;
            }
        }
    }

    private void UpdateWarningFlicker()
    {
        if (warningText == null || !warningText.gameObject.activeSelf)
            return;

        flickerTimer += Time.deltaTime * flickerSpeed;

        float alpha = Mathf.Abs(Mathf.Sin(flickerTimer));
        Color c = warningText.color;
        c.a = alpha;
        warningText.color = c;
    }

    public void RestoreEnergy(float amount)
    {
        energy = Mathf.Clamp(energy + amount, 0f, maxEnergy);

        if (energyBar != null)
            energyBar.value = energy / maxEnergy;

        RefreshBlockBar();
    }

    void GameOver()
    {
        if (gfAnalytics != null)
            gfAnalytics.LogEvent("Death/Respawn", "Energy Depleted");

        Debug.Log("Energy depleted! Game Over.");
        SceneManager.LoadScene("RetryScene");
    }
}