// ===================================================
// FILE: PlacedItem.cs
// Plain data model — think of it as a DTO, not a GameObject.
// No UnityEngine dependency except the Vector2 math struct.
// ===================================================
using UnityEngine;

public class PlacedItem
{
    public string InstanceId;      // unique per placed instance
    public string ItemSchemaId;    // matches ItemSchemaSO.itemId
    public Vector2 WorldPosition;
    public string CurrentRoomId;
    public float CurrentScore;     // last computed score, cached

    public PlacedItem(string instanceId, string schemaId, Vector2 pos, string roomId)
    {
        InstanceId = instanceId;
        ItemSchemaId = schemaId;
        WorldPosition = pos;
        CurrentRoomId = roomId;
    }
}