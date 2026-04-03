using UnityEngine;
using UnityEngine.UI;

public class Checkpoint5Controller : MonoBehaviour
{
    [Header("Checkpoint 5 Spawn")]
    [SerializeField] private Transform checkpoint5Spawn;

    [Header("Player")]
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    [SerializeField] private Transform checkpoint5CameraAnchor;
    [SerializeField] private float cameraYOffset = 10f;
    [SerializeField] private float cameraZ = -10f;

    [Header("Checkpoint 5 Board")]
    [SerializeField] private GameObject checkpoint5BoardRoot;
    [SerializeField] private Transform checkpoint5LettersContainer;
    [SerializeField] private Transform checkpoint5SlotsContainer;
    [SerializeField] private Transform dragLayer;
    [SerializeField] private GameObject draggableLetterPrefab;
    [SerializeField] private GameObject wordSlotPrefab;

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (checkpoint5BoardRoot != null) checkpoint5BoardRoot.SetActive(false);
    }

    public void EnterCheckpoint5()
    {
        if (checkpoint5Spawn == null) return;

        if (playerMovement != null)
            playerMovement.TeleportToStage(checkpoint5Spawn.position);
        else if (playerRb != null)
            playerRb.position = checkpoint5Spawn.position;

        MoveCamera();

        if (checkpoint5BoardRoot != null)
            checkpoint5BoardRoot.SetActive(true);

        FixLayout();
        BuildBoard();
    }

    void FixLayout()
    {
        RectTransform slots = checkpoint5SlotsContainer as RectTransform;
        RectTransform letters = checkpoint5LettersContainer as RectTransform;
        RectTransform drag = dragLayer as RectTransform;

        if (slots)
        {
            slots.anchorMin = slots.anchorMax = new Vector2(0.5f,0.5f);
            slots.anchoredPosition = new Vector2(0, 100);
            slots.localScale = Vector3.one;
        }

        if (letters)
        {
            letters.anchorMin = letters.anchorMax = new Vector2(0.5f,0.5f);
            letters.anchoredPosition = new Vector2(0, -100);
            letters.localScale = Vector3.one;
        }

        if (drag)
        {
            drag.anchorMin = Vector2.zero;
            drag.anchorMax = Vector2.one;
            drag.offsetMin = Vector2.zero;
            drag.offsetMax = Vector2.zero;
            drag.SetAsLastSibling();
        }
    }

    void BuildBoard()
    {
        string word = CollectedLettersInventory.Instance.targetWord.ToUpper();

        foreach (Transform c in checkpoint5SlotsContainer) Destroy(c.gameObject);
        foreach (Transform c in checkpoint5LettersContainer) Destroy(c.gameObject);

        foreach (char c in word)
        {
            var slot = Instantiate(wordSlotPrefab, checkpoint5SlotsContainer);
            slot.GetComponent<CP5WordSlot>()?.SetCorrectLetter(c);
        }

        foreach (char c in CollectedLettersInventory.Instance.GetCollectedLettersInOrder())
        {
            var tile = Instantiate(draggableLetterPrefab, checkpoint5LettersContainer);
            var rt = tile.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.anchoredPosition = Vector2.zero;

            tile.GetComponent<DraggableLetterTile>()?.Init(c, checkpoint5LettersContainer, dragLayer);
        }
    }

    void MoveCamera()
    {
        if (!mainCamera) return;

        Vector3 pos = checkpoint5CameraAnchor ?
            checkpoint5CameraAnchor.position :
            checkpoint5Spawn.position + Vector3.up * cameraYOffset;

        mainCamera.transform.position = new Vector3(pos.x, pos.y, cameraZ);
    }
}
