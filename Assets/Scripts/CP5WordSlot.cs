using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CP5WordSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image background;

    [Header("Colors")]
    [SerializeField] private Color emptyColor = new Color(0.20f, 0.20f, 0.20f, 1f);
    [SerializeField] private Color correctColor = new Color(0.33f, 0.65f, 0.31f, 1f);
    [SerializeField] private Color wrongColor = new Color(0.42f, 0.42f, 0.42f, 1f);

    private char correctLetter;
    private DraggableLetterTile occupant;

    private void Awake()
    {
        if (background == null)
            background = GetComponent<Image>();
    }

    public void SetCorrectLetter(char letter)
    {
        correctLetter = char.ToUpperInvariant(letter);
        ResetVisual();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        DraggableLetterTile tile = eventData.pointerDrag.GetComponent<DraggableLetterTile>();
        if (tile == null)
            return;

        if (occupant != null && occupant != tile)
            occupant.SendBackToHome();

        occupant = tile;
        tile.PlaceInSlot(this);

        if (background != null)
            background.color = tile.Letter == correctLetter ? correctColor : wrongColor;
    }

    public void ClearOccupant(DraggableLetterTile tile)
    {
        if (occupant == tile)
        {
            occupant = null;
            ResetVisual();
        }
    }

    private void ResetVisual()
    {
        if (background != null)
            background.color = emptyColor;
    }
}
