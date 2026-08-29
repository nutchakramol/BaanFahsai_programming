using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelController
{
    private readonly LevelDataSO _levelData;
    private readonly Dictionary<string, ItemSchemaSO> _schemaLookup;
    private readonly List<PlacedItem> _placedItems = new List<PlacedItem>();
    private readonly Dictionary<string, RoomTag> _roomTagLookup;
    private readonly int _totalPaletteItems;

    public LevelController(LevelDataSO levelData, List<ItemSchemaSO> allSchemas)
    {
        _levelData = levelData;

        if (_levelData == null)
        {
            Debug.LogError("[LevelController] LevelDataSO is null.");
            _schemaLookup = new Dictionary<string, ItemSchemaSO>();
            _roomTagLookup = new Dictionary<string, RoomTag>();
            _totalPaletteItems = 0;
            return;
        }

        if (allSchemas == null)
        {
            Debug.LogWarning("[LevelController] allSchemas is null.");
            allSchemas = new List<ItemSchemaSO>();
        }

        // Build schema lookup safely.
        // Ignore null schemas and duplicate/empty IDs.
        _schemaLookup = new Dictionary<string, ItemSchemaSO>();

        foreach (ItemSchemaSO schema in allSchemas)
        {
            if (schema == null)
                continue;

            if (string.IsNullOrWhiteSpace(schema.itemId))
            {
                Debug.LogWarning(
                    $"[LevelController] ItemSchema '{schema.name}' has an empty itemId."
                );
                continue;
            }

            if (_schemaLookup.ContainsKey(schema.itemId))
            {
                Debug.LogWarning(
                    $"[LevelController] Duplicate ItemSchema ID '{schema.itemId}'. " +
                    $"Keeping the first schema."
                );
                continue;
            }

            _schemaLookup.Add(schema.itemId, schema);
        }

        // Build room lookup safely
        _roomTagLookup = new Dictionary<string, RoomTag>();

        if (_levelData.rooms != null)
        {
            foreach (var room in _levelData.rooms)
            {
                if (room == null)
                    continue;

                if (string.IsNullOrWhiteSpace(room.roomId))
                {
                    Debug.LogWarning(
                        "[LevelController] LevelData contains a room with an empty roomId."
                    );
                    continue;
                }

                if (_roomTagLookup.ContainsKey(room.roomId))
                {
                    Debug.LogWarning(
                        $"[LevelController] Duplicate roomId '{room.roomId}'."
                    );
                    continue;
                }

                _roomTagLookup.Add(room.roomId, room.roomTag);
            }
        }

        _totalPaletteItems =
            _levelData.paletteItemIds != null
                ? _levelData.paletteItemIds.Count
                : 0;

        Debug.Log(
            $"[LevelController] Initialized. " +
            $"Schemas: {_schemaLookup.Count}, " +
            $"Rooms: {_roomTagLookup.Count}, " +
            $"Palette Items: {_totalPaletteItems}"
        );
    }

    public void PlaceOrMoveItem(
        string instanceId,
        string schemaId,
        Vector2 worldPos,
        string roomId)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            Debug.LogError(
                "[LevelController] Cannot place item: instanceId is empty."
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(schemaId))
        {
            Debug.LogError(
                $"[LevelController] Cannot place '{instanceId}': schemaId is empty."
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(roomId))
        {
            Debug.LogWarning(
                $"[LevelController] Item '{schemaId}' has no valid roomId."
            );
            return;
        }

        // IMPORTANT:
        // Do not add an unknown item to the tracked list.
        if (!_schemaLookup.ContainsKey(schemaId))
        {
            Debug.LogError(
                $"[LevelController] Unknown schemaId '{schemaId}'. " +
                $"This item is not registered in the ItemSchema list. " +
                $"Check the prefab ID, ItemSchemaSO.itemId, and LevelData."
            );
            return;
        }

        if (!_roomTagLookup.ContainsKey(roomId))
        {
            Debug.LogError(
                $"[LevelController] Unknown roomId '{roomId}' for item '{schemaId}'. " +
                $"Check LevelDataSO.rooms and GridScoringBridge."
            );
            return;
        }

        var existing =
            _placedItems.FirstOrDefault(
                p => p.InstanceId == instanceId
            );

        bool isNewPlacement = existing == null;

        if (existing != null)
        {
            string previousRoom = existing.CurrentRoomId;

            existing.WorldPosition = worldPos;
            existing.CurrentRoomId = roomId;

            if (previousRoom != roomId)
            {
                GameEvents.RaiseItemMovedRoom(
                    existing,
                    previousRoom,
                    roomId
                );
            }
        }
        else
        {
            existing = new PlacedItem(
                instanceId,
                schemaId,
                worldPos,
                roomId
            );

            _placedItems.Add(existing);
        }

        RecalculateAndBroadcast(existing);

        if (isNewPlacement)
        {
            GameEvents.RaisePaletteProgressChanged(
                _placedItems.Count,
                _totalPaletteItems
            );
        }
    }

    /// <summary>
    /// Called when an item is dropped outside any valid room.
    /// Removes it from tracking so the palette can re-show its icon.
    /// </summary>
    public void RemovePlacement(string instanceId)
    {
        int removed =
            _placedItems.RemoveAll(
                p => p.InstanceId == instanceId
            );

        if (removed <= 0)
            return;

        GameEvents.RaisePaletteProgressChanged(
            _placedItems.Count,
            _totalPaletteItems
        );

        var levelResult =
            ScoringEngine.ComputeLevelScore(
                _placedItems,
                _schemaLookup,
                _roomTagLookup,
                _levelData.requirements
            );

        GameEvents.RaiseLevelScoreUpdated(levelResult);
    }

    private void RecalculateAndBroadcast(PlacedItem item)
    {
        if (item == null)
        {
            Debug.LogWarning(
                "[LevelController] Cannot recalculate null item."
            );
            return;
        }

        // SAFE schema lookup
        if (!_schemaLookup.TryGetValue(
                item.ItemSchemaId,
                out ItemSchemaSO schema))
        {
            Debug.LogError(
                $"[LevelController] ItemSchema '{item.ItemSchemaId}' " +
                $"was not found in the schema dictionary."
            );
            return;
        }

        // SAFE room lookup
        if (!_roomTagLookup.TryGetValue(
                item.CurrentRoomId,
                out RoomTag roomTag))
        {
            Debug.LogError(
                $"[LevelController] RoomId '{item.CurrentRoomId}' " +
                $"was not found in LevelDataSO.rooms."
            );
            return;
        }

        item.CurrentScore =
            ScoringEngine.ComputeItemScore(
                item,
                schema,
                roomTag
            );

        GameEvents.RaiseItemScoreUpdated(
            item,
            item.CurrentScore
        );

        var levelResult =
            ScoringEngine.ComputeLevelScore(
                _placedItems,
                _schemaLookup,
                _roomTagLookup,
                _levelData.requirements
            );

        GameEvents.RaiseLevelScoreUpdated(levelResult);
    }

    /// <summary>
    /// Called when the player presses "Check Level".
    /// Stars reflect placement quality.
    /// </summary>
    public LevelScoreResult CheckLevel()
    {
        foreach (var item in _placedItems)
        {
            Debug.Log(
                $"[LevelController] Placed item: " +
                $"{item.ItemSchemaId}, " +
                $"RoomId: '{item.CurrentRoomId}'"
            );
        }

        var result =
            ScoringEngine.ComputeLevelScore(
                _placedItems,
                _schemaLookup,
                _roomTagLookup,
                _levelData.requirements
            );

        int stars =
            StarRatingCalculator.ComputeStars(
                result.OverallScorePercent,
                _levelData.starThresholds
            );

        bool canProceed =
            result.OverallScorePercent >=
            _levelData.minScoreToPass;

        GameEvents.RaiseLevelChecked(
            stars,
            result.OverallScorePercent,
            canProceed
        );

        return result;
    }
}