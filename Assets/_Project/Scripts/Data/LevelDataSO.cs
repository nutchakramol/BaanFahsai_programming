// ===================================================
// FILE: LevelDataSO.cs
// Analogy: this is your "seed data" for a level — like a JSON fixture
// loaded to bootstrap a test environment.
// ===================================================
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Decor/Level Data")]
public class LevelDataSO : ScriptableObject
{
    public int levelIndex;
    public string sceneName;
    public bool multiRoomEnabled;      // false for lvl 1-2, true for lvl 3-6
    public List<RoomDefinition> rooms = new List<RoomDefinition>();
    public List<LevelRequirement> requirements = new List<LevelRequirement>();
    public int targetScoreToWin = 80;  // e.g. percentage threshold
    [Header("Item Palette")]
    public List<string> paletteItemIds = new List<string>(); // schemaIds to spawn, one instance each
    public List<ItemSchemaSO> itemSchemas = new List<ItemSchemaSO>(); // actual schema references for this level's items
    [Header("Star Rating")]
    public float[] starThresholds = new float[] { 50f, 75f, 95f }; // % needed for 1..3 stars
    public float minScoreToPass = 30f; // minimum overall % required to unlock "Next Level"
    [Header("NPC Intro")]
    public string npcName;
    public Sprite npcPortrait;
    public List<string> npcIntroDialogue = new List<string>();
    public List<string> npcCompletionDialogue = new List<string>();
}

[System.Serializable]
public class RoomDefinition
{
    public string roomId;
    public RoomTag roomTag;
    public Vector2 roomOrigin;         // world-space anchor for this room
    public Vector2 roomSize;
    public List<FixedStructureElement> fixedStructures; // walls/windows - texture-only swaps
}

[System.Serializable]
public class FixedStructureElement
{
    public string structureId;         // "window_north", "floor_base"
    public StructureType type;
    public List<Texture2D> availableSkins; // only visuals are swappable
}

public enum StructureType { Wall, Floor, Window, Door }

[System.Serializable]
public class LevelRequirement
{
    public string requirementId;
    public ItemCategory requiredCategory;
    public int minCount;
    public float minAvgScore; // e.g. items must score >= 0.6 avg to "count"
}