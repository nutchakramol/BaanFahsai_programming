using UnityEngine;

public static class LevelProgress
{
    private const string Key = "HighestUnlockedLevelIndex";
    private const string StarsKeyPrefix = "level_stars_";

    public static int HighestUnlockedLevelIndex
    {
        get => PlayerPrefs.GetInt(Key, 0); // level index 0 (Level 1) always unlocked
        private set => PlayerPrefs.SetInt(Key, value);
    }

    public static bool IsUnlocked(int levelIndex) => levelIndex <= HighestUnlockedLevelIndex;

    public static void UnlockUpTo(int levelIndex)
    {
        if (levelIndex > HighestUnlockedLevelIndex)
        {
            HighestUnlockedLevelIndex = levelIndex;
            PlayerPrefs.Save();
        }
    }

    /// <summary>Returns 0-3 stars earned for a level. 0 if never completed.</summary>
    public static int GetStars(int levelIndex)
    {
        return PlayerPrefs.GetInt(StarsKeyPrefix + levelIndex, 0);
    }

    /// <summary>
    /// Call when a level finishes successfully. Saves the star result
    /// (only overwrites if better than the existing score) and unlocks
    /// the next level.
    /// </summary>
    public static void CompleteLevel(int levelIndex, int starsEarned)
    {
        int existingStars = GetStars(levelIndex);
        if (starsEarned > existingStars)
        {
            PlayerPrefs.SetInt(StarsKeyPrefix + levelIndex, starsEarned);
        }

        UnlockUpTo(levelIndex + 1);
    }

    /// <summary>Debug/testing only — resets progress back to Level 1 only.</summary>
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }
}