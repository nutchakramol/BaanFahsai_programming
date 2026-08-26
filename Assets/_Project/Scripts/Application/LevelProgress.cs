using UnityEngine;

public static class LevelProgress
{
    private const string Key = "HighestUnlockedLevelIndex";

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

    /// <summary>Debug/testing only — resets progress back to Level 1 only.</summary>
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }
}