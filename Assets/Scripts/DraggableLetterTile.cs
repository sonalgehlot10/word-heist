using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableLetterTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public char Letter { get; private set; }

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;
    private RectTransform dragLayerRect;

    private Transform homeParent;
    private int homeSiblingIndex;
    private CP5WordSlot currentSlot;
    private bool droppedOnSlot;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null)
            rootCanvas = rootCanvas.rootCanvas;
    }

    public void Init(char letter, Transform homeParent, Transform dragLayer)
    {
        Letter = char.ToUpperInvariant(letter);
        this.homeParent = homeParent;
        homeSiblingIndex = transform.GetSiblingIndex();

        if (dragLayer != null)
            dragLayerRect = dragLayer as RectTransform;

        TMP_Text text = GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            text.text = Letter.ToString();
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        droppedOnSlot = false;

        if (currentSlot != null)
        {
            currentSlot.ClearOccupant(this);
            currentSlot = null;
        }

        Transform targetParent = dragLayerRect != null ? dragLayerRect : (rootCanvas != null ? rootCanvas.transform : transform.parent);
        if (targetParent != null)
        {
            transform.SetParent(targetParent, true);
            transform.SetAsLastSibling();
        }

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransform targetRect = dragLayerRect != null ? dragLayerRect : (rootCanvas != null ? rootCanvas.transform as RectTransform : null);
        if (targetRect == null)
            return;

        Camera eventCamera = eventData.pressEventCamera;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRect, eventData.position, eventCamera, out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        if (!droppedOnSlot && homeParent != null)
        {
            transform.SetParent(homeParent, false);
            transform.SetSiblingIndex(Mathf.Clamp(homeSiblingIndex, 0, homeParent.childCount - 1));
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    public void PlaceInSlot(CP5WordSlot slot)
    {
        currentSlot = slot;
        droppedOnSlot = true;

        transform.SetParent(slot.transform, false);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }

    public void SendBackToHome()
    {
        currentSlot = null;
        droppedOnSlot = false;

        if (homeParent != null)
        {
            transform.SetParent(homeParent, false);
            transform.SetSiblingIndex(Mathf.Clamp(homeSiblingIndex, 0, homeParent.childCount - 1));
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}
