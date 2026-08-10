using System;

public static class GameEvents
{
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
}