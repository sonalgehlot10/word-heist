using UnityEngine;
using UnityEngine.UI;

// Legacy placeholder so old prefabs/components do not break compilation.
// CP5 now uses a single reorderable row of DraggableLetterTile objects.
[RequireComponent(typeof(Image))]
public class CP5WordSlot : MonoBehaviour
{
    private void Awake()
    {
        Image img = GetComponent<Image>();
        if (img != null && img.color.a == 0f)
            img.color = new Color(0.32f, 0.32f, 0.32f, 1f);
    }
}
