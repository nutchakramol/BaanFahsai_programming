using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelController
{
    private readonly LevelDataSO _levelData;
    private readonly Dictionary<string, ItemSchemaSO> _schemaLookup;
    private readonly List<PlacedItem> _placedItems = new List<PlacedItem>();
    private readonly Dictionary<string, RoomTag> _roomTagLookup;

    public LevelController(LevelDataSO levelData, List<ItemSchemaSO> allSchemas)
    {
        _levelData = levelData;
        _schemaLookup = allSchemas.ToDictionary(s => s.itemId, s => s);
        _roomTagLookup = levelData.rooms.ToDictionary(r => r.roomId, r => r.roomTag);
    }

    public void PlaceOrMoveItem(string instanceId, string schemaId, Vector2 worldPos, string roomId)
    {
        var existing = _placedItems.FirstOrDefault(p => p.InstanceId == instanceId);

        if (existing != null)
        {
            string previousRoom = existing.CurrentRoomId;
            existing.WorldPosition = worldPos;
            existing.CurrentRoomId = roomId;

            if (previousRoom != roomId)
                GameEvents.RaiseItemMovedRoom(existing, previousRoom, roomId);
        }
        else
        {
            existing = new PlacedItem(instanceId, schemaId, worldPos, roomId);
            _placedItems.Add(existing);
        }

        RecalculateAndBroadcast(existing);
    }

    private void RecalculateAndBroadcast(PlacedItem item)
    {
        var schema = _schemaLookup[item.ItemSchemaId];
        var roomTag = _roomTagLookup[item.CurrentRoomId];

        item.CurrentScore = ScoringEngine.ComputeItemScore(item, schema, roomTag);
        GameEvents.RaiseItemScoreUpdated(item, item.CurrentScore);

        var levelResult = ScoringEngine.ComputeLevelScore(
            _placedItems, _schemaLookup, _roomTagLookup, _levelData.requirements);

        GameEvents.RaiseLevelScoreUpdated(levelResult);

        if (levelResult.OverallScorePercent >= _levelData.targetScoreToWin)
            GameEvents.RaiseLevelCompleted();
    }
}