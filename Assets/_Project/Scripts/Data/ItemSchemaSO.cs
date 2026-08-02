// ===================================================
// FILE: ItemSchemaSO.cs
// Analogy: this is a "table schema" for a decoration item type.
// ===================================================
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewItemSchema", menuName = "Decor/Item Schema")]
public class ItemSchemaSO : ScriptableObject
{
    [Header("Identity")]
    public string itemId;              // stable key, like a primary key
    public string displayName;
    public Sprite icon;
    public GameObject prefab;

    [Header("Placement Rules")]
    public ItemCategory category;      // e.g. Seating, Lighting, Rug
    public Vector2 footprintSize = Vector2.one; // in world units / grid cells

    [Header("Scoring Data")]
    // A single item can have MULTIPLE ideal spots — each with its own
    // radius and falloff curve. This models "multiple heat points" per item.
    public List<HeatZoneDefinition> heatZones = new List<HeatZoneDefinition>();

    // Functional appropriateness — e.g. "Lamp" scores well only in
    // rooms tagged "NeedsLighting". Decouples logic from raw distance.
    public List<RoomTag> appropriateRoomTags = new List<RoomTag>();
}

public enum ItemCategory { Seating, Lighting, Rug, Storage, Decor, Plant }
public enum RoomTag { LivingRoom, Bedroom, Kitchen, Bathroom, Study }

[System.Serializable]
public class HeatZoneDefinition
{
    public string zoneLabel;           // e.g. "Beside Sofa"
    public Vector2 worldPosition;      // set per-level, or relative anchor
    public float innerRadius = 0.5f;   // full score (1.0) inside this
    public float outerRadius = 2.0f;   // score decays to 0 at this distance
    public AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public float weight = 1f;          // if item has multiple zones, relative importance
}