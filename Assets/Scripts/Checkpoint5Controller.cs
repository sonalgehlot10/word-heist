using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Checkpoint5Controller : MonoBehaviour
{
    [Header("Checkpoint 5 Spawn")]
    [SerializeField] private Transform checkpoint5Spawn;

    [Header("Player")]
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private EnergyBar energyBar;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform checkpoint5CameraAnchor;
    [SerializeField] private float cameraYOffset = 10f;
    [SerializeField] private float cameraZ = -10f;

    [Header("Checkpoint 5 Board")]
    [SerializeField] private GameObject checkpoint5BoardRoot;
    [SerializeField] private Transform checkpoint5LettersContainer;
    [SerializeField] private Transform dragLayer;
    [SerializeField] private GameObject draggableLetterPrefab;

    [Header("Board Layout")]
    [SerializeField] private Vector2 lettersAreaAnchoredPosition = Vector2.zero;
    [SerializeField] private float tileSpacing = 10f;
    [SerializeField] private Vector2 tileSize = new Vector2(60f, 60f);

    [Header("Static Text UI")]
    [SerializeField] private TMP_Text introText;
    [SerializeField] private TMP_Text winText;
    [SerializeField] private TMP_Text loseText;

    [Header("Navigation Buttons")]
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private string nextLevelSceneName = "";

    [TextArea(2, 3)]
    [SerializeField] private string introMessage = "Form the word to advance";

    [TextArea(1, 2)]
    [SerializeField] private string winMessage = "You won!";

    [TextArea(1, 2)]
    [SerializeField] private string loseMessage = "You lost!";

    private readonly List<DraggableLetterTile> tiles = new List<DraggableLetterTile>();
    private bool hasEntered;
    private bool endStateShown;

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (playerRb == null && playerMovement != null)
            playerRb = playerMovement.GetComponent<Rigidbody2D>();

        if (energyBar == null)
            energyBar = FindFirstObjectByType<EnergyBar>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (checkpoint5BoardRoot != null)
            checkpoint5BoardRoot.SetActive(false);

        if (introText != null)
        {
            introText.text = introMessage;
            introText.gameObject.SetActive(false);
        }

        if (winText != null)
        {
            winText.text = winMessage;
            winText.gameObject.SetActive(false);
        }

        if (loseText != null)
        {
            loseText.text = loseMessage;
            loseText.gameObject.SetActive(false);
        }
        if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(false);
        if (nextLevelButton != null) nextLevelButton.gameObject.SetActive(false);
    }

    public void EnterCheckpoint5()
    {
        if (checkpoint5Spawn == null)
        {
            Debug.LogError("[Checkpoint5Controller] checkpoint5Spawn is not assigned.");
            return;
        }

        hasEntered = true;
        endStateShown = false;

        DisableCameraFollowers();
        TeleportPlayerToCheckpoint5();
        MoveCameraToCheckpoint5();
        HidePlayer();

        if (checkpoint5BoardRoot != null)
            checkpoint5BoardRoot.SetActive(true);

        HideResultTexts();
        ShowIntroText();

        ApplyBoardLayout();
        BuildCheckpoint5Board();
        RefreshTileStates();
    }

    private void ShowIntroText()
    {
        if (introText == null)
            return;

        introText.text = introMessage;
        introText.gameObject.SetActive(true);
    }

    private void HideResultTexts()
    {
        if (winText != null)
            winText.gameObject.SetActive(false);

        if (loseText != null)
            loseText.gameObject.SetActive(false);
    }

    private void HidePlayer()
    {
        GameObject playerObject = null;

        if (playerMovement != null)
            playerObject = playerMovement.gameObject;
        else if (playerRb != null)
            playerObject = playerRb.gameObject;

        if (playerObject == null)
        {
            Debug.LogWarning("[Checkpoint5Controller] Could not find player object to hide.");
            return;
        }

        foreach (Renderer renderer in playerObject.GetComponentsInChildren<Renderer>(true))
            renderer.enabled = false;

        foreach (Collider2D collider in playerObject.GetComponentsInChildren<Collider2D>(true))
            collider.enabled = false;

        if (energyBar != null)
            energyBar.SetVisible(false);

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }

        if (playerMovement != null)
            playerMovement.enabled = false;
    }

    private void TeleportPlayerToCheckpoint5()
    {
        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (playerMovement != null)
        {
            playerMovement.TeleportToStage(checkpoint5Spawn.position);
            return;
        }

        if (playerRb != null)
        {
            Vector3 target = new Vector3(checkpoint5Spawn.position.x, checkpoint5Spawn.position.y, playerRb.transform.position.z);
            playerRb.position = target;
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }
        else
        {
            Debug.LogWarning("[Checkpoint5Controller] No player movement or rigidbody found to teleport.");
        }
    }

    public void BuildCheckpoint5Board()
    {
        if (CollectedLettersInventory.Instance == null)
        {
            Debug.LogWarning("[Checkpoint5Controller] CollectedLettersInventory instance not found.");
            return;
        }

        if (checkpoint5LettersContainer == null)
        {
            Debug.LogWarning("[Checkpoint5Controller] checkpoint5LettersContainer is not assigned.");
            return;
        }

        if (draggableLetterPrefab == null)
        {
            Debug.LogWarning("[Checkpoint5Controller] draggableLetterPrefab is not assigned.");
            return;
        }

        ApplyBoardLayout();
        ClearChildren(checkpoint5LettersContainer);
        tiles.Clear();

        char[] collectedLetters = CollectedLettersInventory.Instance.GetCollectedLettersInOrder();
        if (collectedLetters == null || collectedLetters.Length == 0)
            return;

        for (int i = 0; i < collectedLetters.Length; i++)
        {
            GameObject tileObj = Instantiate(draggableLetterPrefab, checkpoint5LettersContainer, false);
            tileObj.name = $"CP5Tile_{i + 1}_{collectedLetters[i]}";

            ApplyTileSizing(tileObj);

            DraggableLetterTile tile = tileObj.GetComponent<DraggableLetterTile>();
            if (tile != null)
            {
                tile.Init(collectedLetters[i], i, checkpoint5LettersContainer, dragLayer, this);
                tiles.Add(tile);
            }
        }

        Canvas.ForceUpdateCanvases();
        ReflowTiles();
    }

    public void RefreshTileStates()
    {
        if (tiles.Count == 0)
            tiles.AddRange(GetTilesFromContainer());

        if (tiles.Count == 0 || CollectedLettersInventory.Instance == null)
            return;

        string targetWord = GetNormalizedTargetWord();
        if (string.IsNullOrEmpty(targetWord))
            return;

        tiles.Sort((a, b) => a.SlotIndex.CompareTo(b.SlotIndex));

        for (int i = 0; i < tiles.Count; i++)
        {
            DraggableLetterTile tile = tiles[i];
            if (tile == null)
                continue;

            bool correct = i < targetWord.Length && char.ToUpperInvariant(tile.Letter) == targetWord[i];

            if (correct)
                tile.SetCorrectVisual();
            else
                tile.SetWrongVisual();
        }

        ReflowTiles();

        if (!endStateShown && IsWordSolved())
            ShowWinText();
    }

    public void OnTileReordered()
    {
        RefreshTileStates();
    }

    public bool CanTileMove(DraggableLetterTile tile)
    {
        return !endStateShown && tile != null && !tile.IsLocked;
    }

    public bool TrySwapTiles(DraggableLetterTile a, DraggableLetterTile b)
    {
        if (endStateShown)
            return false;

        if (a == null || b == null)
            return false;

        if (a.IsLocked || b.IsLocked)
            return false;

        int temp = a.SlotIndex;
        a.SetSlotIndex(b.SlotIndex);
        b.SetSlotIndex(temp);

        RefreshTileStates();
        return true;
    }

    private void ReflowTiles()
    {
        if (checkpoint5LettersContainer == null)
            return;

        tiles.Sort((a, b) => a.SlotIndex.CompareTo(b.SlotIndex));

        float totalWidth = tiles.Count * tileSize.x + Mathf.Max(0, tiles.Count - 1) * tileSpacing;
        float startX = -(totalWidth - tileSize.x) * 0.5f;

        for (int i = 0; i < tiles.Count; i++)
        {
            DraggableLetterTile tile = tiles[i];
            if (tile == null)
                continue;

            tile.SnapToSlot(new Vector2(startX + i * (tileSize.x + tileSpacing), 0f), checkpoint5LettersContainer);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(checkpoint5LettersContainer as RectTransform);
    }

    private void ApplyBoardLayout()
    {
        SetupBoardRoot();
        SetupLettersContainer();
        SetupDragLayer();
        HideLegacySlotsIfPresent();
    }

    private void SetupBoardRoot()
    {
        if (checkpoint5BoardRoot == null)
            return;

        RectTransform rootRT = checkpoint5BoardRoot.GetComponent<RectTransform>();
        if (rootRT != null)
        {
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;
            rootRT.localScale = Vector3.one;
        }

        HorizontalLayoutGroup rootLayout = checkpoint5BoardRoot.GetComponent<HorizontalLayoutGroup>();
        if (rootLayout != null)
            rootLayout.enabled = false;

        ContentSizeFitter rootFitter = checkpoint5BoardRoot.GetComponent<ContentSizeFitter>();
        if (rootFitter != null)
            rootFitter.enabled = false;
    }

    private void SetupLettersContainer()
    {
        if (checkpoint5LettersContainer == null)
            return;

        RectTransform rt = checkpoint5LettersContainer as RectTransform;
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = lettersAreaAnchoredPosition;
            rt.localScale = Vector3.one;
        }

        HorizontalLayoutGroup layout = checkpoint5LettersContainer.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
            layout.enabled = false;

        ContentSizeFitter fitter = checkpoint5LettersContainer.GetComponent<ContentSizeFitter>();
        if (fitter != null)
            fitter.enabled = false;
    }

    private void SetupDragLayer()
    {
        if (dragLayer == null)
            return;

        RectTransform rt = dragLayer as RectTransform;
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        dragLayer.SetAsLastSibling();

        HorizontalLayoutGroup layout = dragLayer.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
            layout.enabled = false;

        ContentSizeFitter fitter = dragLayer.GetComponent<ContentSizeFitter>();
        if (fitter != null)
            fitter.enabled = false;
    }

    private void HideLegacySlotsIfPresent()
    {
        if (checkpoint5BoardRoot == null)
            return;

        Transform slots = checkpoint5BoardRoot.transform.Find("SlotsArea");
        if (slots != null)
            slots.gameObject.SetActive(false);
    }

    private void ApplyTileSizing(GameObject obj)
    {
        if (obj == null)
            return;

        RectTransform rt = obj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.sizeDelta = tileSize;
        }

        LayoutElement layout = obj.GetComponent<LayoutElement>();
        if (layout == null)
            layout = obj.AddComponent<LayoutElement>();

        layout.ignoreLayout = true;
        layout.minWidth = tileSize.x;
        layout.minHeight = tileSize.y;
        layout.preferredWidth = tileSize.x;
        layout.preferredHeight = tileSize.y;
        layout.flexibleWidth = 0f;
        layout.flexibleHeight = 0f;
    }

    private void ClearChildren(Transform parent)
    {
        if (parent == null)
            return;

        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }

    private string GetNormalizedTargetWord()
    {
        if (CollectedLettersInventory.Instance == null)
            return string.Empty;

        string word = CollectedLettersInventory.Instance.targetWord;
        return string.IsNullOrWhiteSpace(word) ? string.Empty : word.Trim().ToUpperInvariant();
    }

    private List<DraggableLetterTile> GetTilesFromContainer()
    {
        List<DraggableLetterTile> result = new List<DraggableLetterTile>();
        if (checkpoint5LettersContainer == null)
            return result;

        for (int i = 0; i < checkpoint5LettersContainer.childCount; i++)
        {
            DraggableLetterTile tile = checkpoint5LettersContainer.GetChild(i).GetComponent<DraggableLetterTile>();
            if (tile != null)
                result.Add(tile);
        }

        return result;
    }

    private bool IsWordSolved()
    {
        string targetWord = GetNormalizedTargetWord();
        if (string.IsNullOrEmpty(targetWord))
            return false;

        if (tiles.Count != targetWord.Length)
            return false;

        tiles.Sort((a, b) => a.SlotIndex.CompareTo(b.SlotIndex));

        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] == null)
                return false;

            if (char.ToUpperInvariant(tiles[i].Letter) != targetWord[i])
                return false;
        }

        return true;
    }

    private void MoveCameraToCheckpoint5()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogWarning("[Checkpoint5Controller] Main camera not found.");
            return;
        }

        Vector3 targetPosition;
        if (checkpoint5CameraAnchor != null)
        {
            targetPosition = new Vector3(checkpoint5CameraAnchor.position.x, checkpoint5CameraAnchor.position.y, cameraZ);
        }
        else
        {
            targetPosition = new Vector3(checkpoint5Spawn.position.x, checkpoint5Spawn.position.y + cameraYOffset, cameraZ);
        }

        mainCamera.transform.position = targetPosition;
    }

    private void DisableCameraFollowers()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        MonoBehaviour[] behaviours = mainCamera.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null || !behaviour.enabled)
                continue;

            string typeName = behaviour.GetType().Name;
            if (typeName.Contains("Follow") || typeName.Contains("Camera") || typeName.Contains("Cinemachine"))
                behaviour.enabled = false;
        }
    }

    public void ShowWinText()
    {
        if (endStateShown)
            return;

        endStateShown = true;

        if (winText != null)
        {
            winText.text = winMessage;
            winText.gameObject.SetActive(true);
        }
        if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(true);
        if (nextLevelButton != null) nextLevelButton.gameObject.SetActive(!string.IsNullOrEmpty(nextLevelSceneName));
    }

    public void ShowLoseText()
    {
        if (endStateShown)
            return;

        endStateShown = true;

        if (loseText != null)
        {
            loseText.text = loseMessage;
            loseText.gameObject.SetActive(true);
        }
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void GoToNextLevel()
    {
        if (!string.IsNullOrEmpty(nextLevelSceneName))
            SceneManager.LoadScene(nextLevelSceneName);
    }

    public bool HasEnteredCheckpoint5 => hasEntered;
}
