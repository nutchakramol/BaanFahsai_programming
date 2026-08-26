using UnityEngine;

public class FurnitureItem : MonoBehaviour
{
    [Header("Identity")]
    public string furnitureID;
    public string furnitureName;

    [Header("Grid Footprint")]
    public Vector2Int gridSize = new Vector2Int(1, 1); // width x height in cells

    [Header("Placement Rules")]
    public bool canRotate = true;
    public int levelUnlock = 1; // which level this furniture unlocks at

    [Header("Customization (optional)")]
    public Material[] colorVariants;

    private int currentVariantIndex = 0;

    public void SetColorVariant(int index)
    {
        if (colorVariants == null || colorVariants.Length == 0) return;
        if (index < 0 || index >= colorVariants.Length) return;

        currentVariantIndex = index;
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.material = colorVariants[index];
        }
    }
}