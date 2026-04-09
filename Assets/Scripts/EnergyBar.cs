using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnergyBar : MonoBehaviour
{
    [Header("Energy Settings")]
    public float maxEnergy = 100f;
    public float drainPerUnit = 0.2f;
    public float verticalWeight = 0.4f;

    [Header("Filtering")]
    public float minMovementThreshold = 0.01f;
    public float axisDeadZone = 0.1f;

    [Header("Bar Settings")]
    public Vector3 barLocalOffset = new Vector3(0f, 1.2f, 0f);
    public Vector2 barSize = new Vector2(140f, 18f);
    public float worldScale = 0.01f;
    public float borderThickness = 2f;

    [Header("Colors")]
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.35f);
    public Color borderColor = new Color(1f, 1f, 1f, 0.18f);

    [Header("Warning Text")]
    public string warningMessage = "Energy draining!";
    public Vector3 warningTextOffset = new Vector3(0f, 0.42f, 0f);
    public int warningFontSize = 55;
    public float flickerSpeed = 6f;
    public float criticalEnergyThreshold = 0.15f;
    public Color warningBaseColor = new Color(1f, 0.15f, 0.15f, 1f);
    public Color warningFlashColor = new Color(1f, 0.55f, 0.55f, 1f);

    private float energy;
    private Vector2 lastPos;
    private Rigidbody2D rb;
    private GoogleFormAnalytics gfAnalytics;

    private Transform barRoot;
    private Canvas barCanvas;
    private Slider slider;
    private Image backgroundImage;
    private Image borderImage;
    private Image fillImage;

    private Sprite whiteSprite;
    private Sprite gradientSprite;

    private TextMesh warningTextMesh;
    private float flickerTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        gfAnalytics = FindObjectOfType<GoogleFormAnalytics>();
    }

    private void Start()
    {
        energy = maxEnergy;
        lastPos = rb != null ? rb.position : (Vector2)transform.position;

        BuildEnergyBar();
        RefreshBar();
    }

    private void LateUpdate()
    {
        Vector2 currentPos = rb != null ? rb.position : (Vector2)transform.position;

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
            RefreshBar();

            if (energy <= 0f)
                GameOver();
        }

        lastPos = currentPos;

        if (barRoot != null)
            barRoot.localPosition = barLocalOffset;

        UpdateWarningFlicker();
    }

    private void BuildEnergyBar()
    {
        if (barRoot != null)
            return;

        whiteSprite = CreateWhiteSprite();
        gradientSprite = CreateGradientSprite();

        GameObject root = new GameObject(
            "EnergyBarRoot",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        root.transform.SetParent(transform, false);
        root.transform.localPosition = barLocalOffset;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one * worldScale;

        barRoot = root.transform;

        RectTransform rootRT = root.GetComponent<RectTransform>();
        rootRT.sizeDelta = barSize;

        barCanvas = root.GetComponent<Canvas>();
        barCanvas.renderMode = RenderMode.WorldSpace;
        barCanvas.worldCamera = Camera.main;
        barCanvas.overrideSorting = true;
        barCanvas.sortingOrder = 200;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        // Border
        GameObject border = new GameObject("Border", typeof(RectTransform), typeof(Image));
        border.transform.SetParent(barRoot, false);

        RectTransform borderRT = border.GetComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = Vector2.one;
        borderRT.offsetMin = Vector2.zero;
        borderRT.offsetMax = Vector2.zero;

        borderImage = border.GetComponent<Image>();
        borderImage.sprite = whiteSprite;
        borderImage.color = borderColor;
        borderImage.type = Image.Type.Sliced;

        // Background
        GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(barRoot, false);

        RectTransform backgroundRT = background.GetComponent<RectTransform>();
        backgroundRT.anchorMin = Vector2.zero;
        backgroundRT.anchorMax = Vector2.one;
        backgroundRT.offsetMin = new Vector2(borderThickness, borderThickness);
        backgroundRT.offsetMax = new Vector2(-borderThickness, -borderThickness);

        backgroundImage = background.GetComponent<Image>();
        backgroundImage.sprite = whiteSprite;
        backgroundImage.color = backgroundColor;
        backgroundImage.type = Image.Type.Sliced;

        // Fill
        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(background.transform, false);

        RectTransform fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        fillImage = fill.GetComponent<Image>();
        fillImage.sprite = gradientSprite;
        fillImage.color = Color.white;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 1f;

        // Warning text as TextMesh so it remains visible in world space
        GameObject warningObj = new GameObject("EnergyWarningText");
        warningObj.transform.SetParent(barRoot, false);
        warningObj.transform.localPosition = warningTextOffset;
        warningObj.transform.localRotation = Quaternion.identity;

        float inverseScale = worldScale > 0.0001f ? (1f / worldScale) : 100f;
        warningObj.transform.localScale = Vector3.one * inverseScale;

        warningTextMesh = warningObj.AddComponent<TextMesh>();
        warningTextMesh.text = warningMessage;
        warningTextMesh.fontSize = warningFontSize;
        warningTextMesh.characterSize = 0.05f;
        warningTextMesh.anchor = TextAnchor.MiddleCenter;
        warningTextMesh.alignment = TextAlignment.Center;
        warningTextMesh.color = new Color(warningBaseColor.r, warningBaseColor.g, warningBaseColor.b, 0f);

        MeshRenderer mr = warningObj.GetComponent<MeshRenderer>();
        if (mr != null)
            mr.sortingOrder = 500;

        warningObj.SetActive(false);
    }

    private Sprite CreateWhiteSprite()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        texture.hideFlags = HideFlags.HideAndDontSave;
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    private Sprite CreateGradientSprite()
    {
        const int width = 256;
        const int height = 1;

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int x = 0; x < width; x++)
        {
            float t = x / (float)(width - 1);

            Color c;
            if (t < 0.5f)
            {
                float localT = t / 0.5f;
                c = Color.Lerp(Color.red, Color.yellow, localT);
            }
            else
            {
                float localT = (t - 0.5f) / 0.5f;
                c = Color.Lerp(Color.yellow, Color.green, localT);
            }

            texture.SetPixel(x, 0, c);
        }

        texture.Apply();
        texture.hideFlags = HideFlags.HideAndDontSave;

        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    private void RefreshBar()
    {
        float normalized = Mathf.Clamp01(energy / maxEnergy);

        if (fillImage != null)
            fillImage.fillAmount = normalized;

        if (warningTextMesh != null)
        {
            bool isCritical = normalized <= criticalEnergyThreshold;
            warningTextMesh.gameObject.SetActive(isCritical);

            if (!isCritical)
            {
                Color c = warningTextMesh.color;
                c.a = 0f;
                warningTextMesh.color = c;
            }
        }
    }

    private void UpdateWarningFlicker()
    {
        if (warningTextMesh == null || !warningTextMesh.gameObject.activeSelf)
            return;

        float normalized = Mathf.Clamp01(energy / maxEnergy);
        if (normalized > criticalEnergyThreshold)
            return;

        flickerTimer += Time.deltaTime * flickerSpeed;

        float pulse = Mathf.Abs(Mathf.Sin(flickerTimer));
        Color c = Color.Lerp(warningBaseColor, warningFlashColor, pulse);
        c.a = Mathf.Lerp(0.35f, 1f, pulse);
        warningTextMesh.color = c;
    }

    public void RestoreEnergy(float amount)
    {
        energy = Mathf.Clamp(energy + amount, 0f, maxEnergy);
        RefreshBar();
    }

    private void GameOver()
    {
        if (gfAnalytics != null)
            gfAnalytics.LogEvent("Death/Respawn", "Energy Depleted");

        Debug.Log("Energy depleted! Game Over.");
        SceneManager.LoadScene("RetryScene");
    }
}