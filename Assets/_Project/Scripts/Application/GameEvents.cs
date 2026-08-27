using System;

public static class GameEvents
{
    // Persists across scene loads so the next scene can read which level was selected.
    public static LevelDataSO CurrentLevelData { get; private set; }

    public static event Action<PlacedItem, float> OnItemScoreUpdated;
    public static void RaiseItemScoreUpdated(PlacedItem item, float score)
        => OnItemScoreUpdated?.Invoke(item, score);

    public static event Action<LevelScoreResult> OnLevelScoreUpdated;
    public static void RaiseLevelScoreUpdated(LevelScoreResult result)
        => OnLevelScoreUpdated?.Invoke(result);

    public static event Action OnLevelCompleted;
    public static void RaiseLevelCompleted() => OnLevelCompleted?.Invoke();

    public static event Action<PlacedItem, string, string> OnItemMovedRoom;
    public static void RaiseItemMovedRoom(PlacedItem item, string from, string to)
        => OnItemMovedRoom?.Invoke(item, from, to);
    
    public static event Action<int, int> OnPaletteProgressChanged;
    public static void RaisePaletteProgressChanged(int placed, int total)
        => OnPaletteProgressChanged?.Invoke(placed, total);

    public static event Action<int, float, bool> OnLevelChecked;
    public static void RaiseLevelChecked(int stars, float overallPercent, bool canProceed)
        => OnLevelChecked?.Invoke(stars, overallPercent, canProceed);

    public static event Action<LevelDataSO> OnLevelSelected;
    public static void RaiseLevelSelected(LevelDataSO level)
    {
        CurrentLevelData = level; // remember it, since subscribers in the next scene miss the event itself
        OnLevelSelected?.Invoke(level);
    }
}