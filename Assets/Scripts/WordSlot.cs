using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class WordSlot : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image background;
    public TMP_Text letterText;
    public Outline outline;

    private bool draggable = true;
    private CanvasGroup canvasGroup;

    private GameObject dragTile;
    private RectTransform dragTileRect;
    private Canvas rootCanvas;

    private string pickedLetter = "";

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (outline != null)
            outline.enabled = false;

        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void Setup(char letter)
    {
        if (letterText != null)
            letterText.text = letter.ToString();
    }

    public char GetLetter()
    {
        if (letterText == null || string.IsNullOrEmpty(letterText.text))
            return ' ';

        return letterText.text[0];
    }

    public void SetGreen()
    {
        if (background != null)
            background.color = Color.green;

        draggable = false;
    }

    public void SetYellow()
    {
        if (background != null)
            background.color = Color.yellow;

        draggable = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!draggable)
            return;

        if (letterText == null || background == null)
            return;

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas == null)
            return;

        pickedLetter = letterText.text;
        letterText.text = "";

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;

        dragTile = new GameObject("DragTile", typeof(RectTransform));
        dragTile.transform.SetParent(rootCanvas.transform, false);
        dragTileRect = dragTile.GetComponent<RectTransform>();

        Image img = dragTile.AddComponent<Image>();
        if (background.sprite != null)
        {
            img.sprite = background.sprite;
            img.type = background.type;
            img.preserveAspect = background.preserveAspect;
        }
        else
        {
            img.color = background.color;
        }

        img.raycastTarget = false;

        GameObject textObj = new GameObject("TileText", typeof(RectTransform));
        textObj.transform.SetParent(dragTile.transform, false);

        TMP_Text dragText = textObj.AddComponent<TextMeshProUGUI>();
        dragText.text = pickedLetter;
        dragText.alignment = TextAlignmentOptions.Center;
        dragText.enableAutoSizing = true;
        dragText.raycastTarget = false;

        if (background.rectTransform != null)
            dragTileRect.sizeDelta = background.rectTransform.sizeDelta;

        dragTile.transform.SetAsLastSibling();
        UpdateDragTilePosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragTile == null)
            return;

        UpdateDragTilePosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        if (dragTile != null)
        {
            Destroy(dragTile);
            dragTile = null;
        }

        if (letterText != null && string.IsNullOrEmpty(letterText.text))
            letterText.text = pickedLetter;

        pickedLetter = "";
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        WordSlot sourceSlot = eventData.pointerDrag.GetComponent<WordSlot>();
        if (sourceSlot == null)
            return;

        if (sourceSlot == this)
            return;

        if (!this.draggable || !sourceSlot.draggable)
            return;

        if (letterText == null || sourceSlot.letterText == null)
            return;

        string sourceLetter = sourceSlot.pickedLetter;
        string targetLetter = letterText.text;

        letterText.text = sourceLetter;
        sourceSlot.letterText.text = targetLetter;

        sourceSlot.pickedLetter = "";

        if (sourceSlot.dragTile != null)
            Destroy(sourceSlot.dragTile);

        if (WordGameManager.Instance != null)
            WordGameManager.Instance.CheckWord();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (outline != null)
            outline.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (outline != null)
            outline.enabled = false;
    }

    private void UpdateDragTilePosition(PointerEventData eventData)
    {
        if (dragTileRect == null || rootCanvas == null)
            return;

        RectTransform canvasRect = rootCanvas.transform as RectTransform;
        Vector2 localPoint;

        if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, eventData.position, null, out localPoint);
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, eventData.position, rootCanvas.worldCamera, out localPoint);
        }

        dragTileRect.anchoredPosition = localPoint;
    }
}
