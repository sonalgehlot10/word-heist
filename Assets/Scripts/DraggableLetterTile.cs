using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class DraggableLetterTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public char Letter { get; private set; }
    public bool IsLocked { get; private set; }
    public int SlotIndex => slotIndex;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Image backgroundImage;
    private TMP_Text letterText;

    private RectTransform dragLayerRect;
    private Transform homeParent;
    private Checkpoint5Controller controller;

    private int slotIndex;
    private bool dropHandled;

    [Header("Visuals")]
    [SerializeField] private Color defaultBackground = new Color(0.82f, 0.72f, 0.25f, 1f);
    [SerializeField] private Color correctBackground = new Color(0.35f, 0.70f, 0.35f, 1f);
    [SerializeField] private Color lockedTextColor = Color.white;
    [SerializeField] private Color unlockedTextColor = Color.white;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        backgroundImage = GetComponent<Image>();
        letterText = GetComponentInChildren<TMP_Text>(true);

        if (letterText != null)
            letterText.raycastTarget = false;
    }

    public void Init(char letter, int initialSlotIndex, Transform homeParent, Transform dragLayer, Checkpoint5Controller controller)
    {
        Letter = char.ToUpperInvariant(letter);
        slotIndex = initialSlotIndex;
        this.homeParent = homeParent;
        this.controller = controller;
        dragLayerRect = dragLayer as RectTransform;

        if (letterText == null)
            letterText = GetComponentInChildren<TMP_Text>(true);

        if (letterText != null)
        {
            letterText.text = Letter.ToString();
            letterText.raycastTarget = false;
        }

        if (backgroundImage != null)
            backgroundImage.raycastTarget = true;

        SetWrongVisual();
    }

    public void SetSlotIndex(int newIndex)
    {
        slotIndex = Mathf.Max(0, newIndex);
    }

    public void SnapToSlot(Vector2 localPosition, Transform parent)
    {
        if (parent != null && transform.parent != parent)
            transform.SetParent(parent, false);

        rectTransform.anchoredPosition = localPosition;
        rectTransform.localScale = Vector3.one;
    }

    public void SetCorrectVisual()
    {
        IsLocked = true;

        if (backgroundImage != null)
        {
            backgroundImage.color = correctBackground;
            backgroundImage.raycastTarget = false;
        }

        if (letterText != null)
            letterText.color = lockedTextColor;

        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void SetWrongVisual()
    {
        IsLocked = false;

        if (backgroundImage != null)
        {
            backgroundImage.color = defaultBackground;
            backgroundImage.raycastTarget = true;
        }

        if (letterText != null)
            letterText.color = unlockedTextColor;

        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dropHandled = false;

        if (IsLocked)
            return;

        if (controller != null && !controller.CanTileMove(this))
            return;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;

        if (homeParent == null)
            homeParent = transform.parent;

        if (dragLayerRect != null)
            transform.SetParent(dragLayerRect, true);

        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsLocked)
            return;

        if (dragLayerRect == null)
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragLayerRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        if (IsLocked)
            return;

        if (!dropHandled && controller != null)
            controller.RefreshTileStates();
        else if (!dropHandled)
            ReturnToHomePosition();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (IsLocked)
            return;

        if (eventData == null || eventData.pointerDrag == null)
            return;

        DraggableLetterTile draggedTile = eventData.pointerDrag.GetComponent<DraggableLetterTile>();
        if (draggedTile == null || draggedTile == this)
            return;

        if (draggedTile.IsLocked || IsLocked)
            return;

        if (controller == null || draggedTile.controller != controller)
            return;

        bool swapped = controller.TrySwapTiles(this, draggedTile);
        if (swapped)
        {
            dropHandled = true;
            draggedTile.dropHandled = true;
        }
    }

    private void ReturnToHomePosition()
    {
        if (controller != null)
        {
            controller.RefreshTileStates();
            return;
        }

        if (homeParent == null)
            return;

        transform.SetParent(homeParent, false);
        rectTransform.anchoredPosition = Vector2.zero;
    }
}
