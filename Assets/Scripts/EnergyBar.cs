using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnergyBar : MonoBehaviour
{
    [Header("Legacy UI")]
    [SerializeField] private Slider energyBar; // optional legacy reference

    [Header("Energy Settings")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float drainPerUnit = 0.2f;
    [SerializeField] private float verticalWeight = 0.4f;

    [Header("Filtering")]
    [SerializeField] private float minMovementThreshold = 0.01f;
    [SerializeField] private float axisDeadZone = 0.1f;

    [Header("Continuous Bar")]
    [SerializeField] private Vector3 barLocalOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private Vector2 barSize = new Vector2(0.4f, 0.22f);
    [SerializeField] private Color healthyColor = new Color(0.20f, 0.90f, 0.20f, 1f); // green
    [SerializeField] private Color warningColor = new Color(0.98f, 0.92f, 0.15f, 1f); // yellow
    [SerializeField] private Color dangerColor = new Color(0.98f, 0.20f, 0.20f, 1f);   // red
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.35f);

    [Header("Warning Text")]
    [SerializeField] private string warningMessage = "Energy draining!";
    [SerializeField] private Vector3 warningTextOffset = new Vector3(0f, 0.42f, 0f);
    [SerializeField] private int warningFontSize = 55;
    [SerializeField] private float warningCharacterSize = 0.05f;
    [SerializeField] private float flickerSpeed = 6f;
    [Range(0.01f, 1f)]
    [SerializeField] private float lowEnergyThreshold = 0.15f;

    [Header("Sorting")]
    [SerializeField] private int backgroundSortingOrder = -20;
    [SerializeField] private int fillSortingOrder = -19;
    [SerializeField] private int textSortingOrder = -18;

    private float energy;
    private Vector2 lastPos;
    private Rigidbody2D rb;
    private GoogleFormAnalytics gfAnalytics;

    private Transform barRoot;
    private Transform fillRoot;
    private SpriteRenderer fillRenderer;
    private TextMesh warningText;
    private float flickerTimer = 0f;

    private static Sprite whiteSprite;

    private Texture2D fillTexture;
    private Sprite fillSprite;
    private const int GradientWidth = 256;
    private const int GradientHeight = 16;
    private float lastFillT = -1f;

    private static Material unlitMaterial;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        gfAnalytics = FindObjectOfType<GoogleFormAnalytics>();
    }

    private void Start()
    {
        energy = maxEnergy;
        lastPos = rb ? rb.position : (Vector2)transform.position;

        if (energyBar != null)
            energyBar.gameObject.SetActive(false);

        BuildContinuousBar();
        RefreshContinuousBar(true);
    }

    private void LateUpdate()
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

            RefreshContinuousBar();

            if (energy <= 0f)
            {
                GameOver();
                return;
            }
        }

        lastPos = currentPos;

        if (barRoot != null)
            barRoot.localPosition = barLocalOffset;

        UpdateWarningFlicker();
    }

    private void BuildContinuousBar()
    {
        if (barRoot != null)
            return;

        barRoot = new GameObject("EnergyBarRoot").transform;
        barRoot.SetParent(transform, false);
        barRoot.localPosition = barLocalOffset;

        if (whiteSprite == null)
            whiteSprite = CreateWhiteSprite();

        if (unlitMaterial == null)
            unlitMaterial = CreateUnlitMaterial();

        // Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(barRoot, false);
        background.transform.localPosition = Vector3.zero;
        background.transform.localScale = new Vector3(barSize.x + 0.08f, barSize.y + 0.08f, 1f);

        SpriteRenderer bgRenderer = background.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = whiteSprite;
        bgRenderer.color = backgroundColor;
        bgRenderer.sortingOrder = backgroundSortingOrder;
        bgRenderer.material = unlitMaterial;

        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(barRoot, false);
        fill.transform.localPosition = Vector3.zero;
        fill.transform.localScale = new Vector3(barSize.x, barSize.y, 1f);

        fillRenderer = fill.AddComponent<SpriteRenderer>();
        fillRenderer.sortingOrder = fillSortingOrder;
        fillRenderer.color = Color.white;
        fillRenderer.material = unlitMaterial;

        fillRoot = fill.transform;

        fillTexture = new Texture2D(GradientWidth, GradientHeight, TextureFormat.RGBA32, false);
        fillTexture.filterMode = FilterMode.Bilinear;
        fillTexture.wrapMode = TextureWrapMode.Clamp;

        fillSprite = Sprite.Create(
            fillTexture,
            new Rect(0, 0, GradientWidth, GradientHeight),
            new Vector2(0.5f, 0.5f),
            100f
        );

        fillRenderer.sprite = fillSprite;

        // Warning text
        GameObject textObj = new GameObject("EnergyWarningText");
        textObj.transform.SetParent(barRoot, false);
        textObj.transform.localPosition = warningTextOffset;

        warningText = textObj.AddComponent<TextMesh>();
        warningText.text = warningMessage;
        warningText.fontSize = warningFontSize;
        warningText.characterSize = warningCharacterSize;
        warningText.alignment = TextAlignment.Center;
        warningText.anchor = TextAnchor.LowerCenter;
        warningText.color = new Color(1f, 0.2f, 0.2f, 0f);
        warningText.gameObject.SetActive(false);

        MeshRenderer textRenderer = textObj.GetComponent<MeshRenderer>();
        if (textRenderer != null)
        {
            textRenderer.sortingOrder = textSortingOrder;
            textRenderer.sortingLayerName = "Default";
        }
    }

    private void RefreshContinuousBar(bool forceUpdate = false)
    {
        if (fillRoot == null || fillRenderer == null)
            return;

        float t = Mathf.Clamp01(energy / maxEnergy);
        float width = Mathf.Max(0.001f, barSize.x * t);

        // Keep the RIGHT edge fixed so green disappears first,
        // then yellow, then only red remains.
        fillRoot.localPosition = new Vector3(
            barSize.x * 0.5f - width * 0.5f,
            0f,
            0f
        );

        fillRoot.localScale = new Vector3(width, barSize.y, 1f);

        UpdateGradientSlice(t, forceUpdate);

        bool isCritical = t <= lowEnergyThreshold;

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

    // Builds only the visible slice of the gradient.
    // Full energy  -> green -> yellow -> red
    // Medium energy -> yellow -> red
    // Low energy    -> red only
    private void UpdateGradientSlice(float t, bool forceUpdate = false)
    {
        if (fillTexture == null)
            return;

        if (!forceUpdate && Mathf.Abs(t - lastFillT) < 0.0001f)
            return;

        lastFillT = t;

        // We want the right-most part of the full gradient.
        // At full energy, startT = 0 (green -> yellow -> red)
        // At half energy, startT = 0.5 (yellow -> red)
        // At low energy, startT approaches 1 (mostly red)
        float startT = 1f - t;

        for (int x = 0; x < GradientWidth; x++)
        {
            float u = (float)x / (GradientWidth - 1);
            float sourceT = Mathf.Lerp(startT, 1f, u);

            Color c = EvaluateGradient(sourceT);

            for (int y = 0; y < GradientHeight; y++)
                fillTexture.SetPixel(x, y, c);
        }

        fillTexture.Apply(false);
    }

    private Color EvaluateGradient(float t)
    {
        if (t < 0.5f)
            return Color.Lerp(healthyColor, warningColor, t * 2f);

        return Color.Lerp(warningColor, dangerColor, (t - 0.5f) * 2f);
    }

    private Sprite CreateWhiteSprite()
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;

        return Sprite.Create(
            tex,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1f
        );
    }

    private Material CreateUnlitMaterial()
    {
        Shader shader =
            Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ??
            Shader.Find("Sprites/Default");

        if (shader == null)
        {
            Debug.LogWarning("EnergyBar: Could not find an unlit sprite shader. Using default material.");
            return new Material(Shader.Find("Sprites/Default"));
        }

        Material mat = new Material(shader);
        mat.hideFlags = HideFlags.HideAndDontSave;
        return mat;
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

    private void GameOver()
    {
        if (gfAnalytics != null)
            gfAnalytics.LogEvent("Death/Respawn", "Energy Depleted");

        Debug.Log("Energy depleted! Game Over.");
        SceneManager.LoadScene("RetryScene");
    }

    private void OnDestroy()
    {
        if (fillSprite != null)
            Destroy(fillSprite);

        if (fillTexture != null)
            Destroy(fillTexture);

        if (unlitMaterial != null)
            Destroy(unlitMaterial);
    }
}