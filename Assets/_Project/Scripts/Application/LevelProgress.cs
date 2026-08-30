using UnityEngine;
public static class LevelProgress
{
    private const string Key = "HighestUnlockedLevelIndex";
    private const string StarsKeyPrefix = "level_stars_";
    private const string AttemptedKeyPrefix = "level_attempted_";

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

    /// <summary>True if the player has ever completed a check on this level, pass or fail.</summary>
    public static bool HasAttempted(int levelIndex)
    {
        return PlayerPrefs.GetInt(AttemptedKeyPrefix + levelIndex, 0) == 1;
    }

    private static void MarkAttempted(int levelIndex)
    {
        PlayerPrefs.SetInt(AttemptedKeyPrefix + levelIndex, 1);
    }

    /// <summary>
    /// Call when a level finishes successfully. Saves the star result
    /// (only overwrites if better than the existing score) and unlocks
    /// the next level.
    /// </summary>
    public static void CompleteLevel(int levelIndex, int starsEarned)
    {
        MarkAttempted(levelIndex);

        int existingStars = GetStars(levelIndex);
        if (starsEarned > existingStars)
        {
            PlayerPrefs.SetInt(StarsKeyPrefix + levelIndex, starsEarned);
        }
        UnlockUpTo(levelIndex + 1);
        PlayerPrefs.Save();
    }

    /// <summary>Saves star result without unlocking the next level (for failed attempts).</summary>
    public static void SaveStarsOnly(int levelIndex, int starsEarned)
    {
        MarkAttempted(levelIndex);

        int existingStars = GetStars(levelIndex);
        if (starsEarned > existingStars)
        {
            PlayerPrefs.SetInt(StarsKeyPrefix + levelIndex, starsEarned);
        }
        PlayerPrefs.Save();
    }

    /// <summary>Debug/testing only — resets progress back to Level 1 only.</summary>
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }
}