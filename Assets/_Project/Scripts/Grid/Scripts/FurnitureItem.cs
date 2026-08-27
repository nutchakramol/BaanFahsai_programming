using UnityEngine;

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

    [Header("Customization (optional)")]
    public Material[] colorVariants;

    // Returns true if the level this furniture requires has been unlocked.
    // Assumes LevelProgress.IsUnlocked uses 0-indexed levels (matches
    // LevelSelectUI's bubble loop, where bubble i = Level i+1).
    public bool IsUnlockedForPlayer()
    {
        return LevelProgress.IsUnlocked(levelUnlock - 1);
    }
}