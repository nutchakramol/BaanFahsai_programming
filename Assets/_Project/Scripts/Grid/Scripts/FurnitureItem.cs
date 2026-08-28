using UnityEngine;

public enum SurfaceBand { Floor, Wall, Ceiling, Countertop }

public class FurnitureItem : MonoBehaviour
{
    [Header("Identity")]
    public string furnitureID;
    public string furnitureName;

    [Header("Grid Footprint")]
    public Vector2Int gridSize = new Vector2Int(1, 1);

    [Header("Placement Rules")]
    public bool canRotate = true;
    [Tooltip("1-indexed level this furniture unlocks at (Level 1 = bedroom, etc.)")]
    public int levelUnlock = 1;
    [Tooltip("Which surface this item can be placed on")]
    public SurfaceBand surface = SurfaceBand.Floor;

    [Header("Customization (optional)")]
    public Material[] colorVariants;

    public bool IsUnlockedForPlayer()
    {
        return LevelProgress.IsUnlocked(levelUnlock - 1);
    }
}