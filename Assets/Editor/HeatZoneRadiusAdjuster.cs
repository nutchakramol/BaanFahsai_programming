using UnityEngine;
using UnityEditor;

public static class HeatZoneRadiusAdjuster
{
    [MenuItem("Tools/Decor/Loosen All Heat Zone Radii (All Levels)")]
    private static void LoosenAllLevels()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemSchemaSO", new[] { "Assets/_Project/ScriptableObjects/ItemSchemas" });

        int updatedZones = 0;
        int updatedItems = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemSchemaSO schema = AssetDatabase.LoadAssetAtPath<ItemSchemaSO>(path);
            if (schema == null || schema.heatZones == null || schema.heatZones.Count == 0) continue;

            foreach (var zone in schema.heatZones)
            {
                zone.innerRadius = 2f;
                zone.outerRadius = 5f;
                updatedZones++;
            }

            EditorUtility.SetDirty(schema);
            updatedItems++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[HeatZoneRadiusAdjuster] Updated {updatedZones} heat zones across {updatedItems} items (all levels).");
    }

    [MenuItem("Tools/Decor/Loosen All Heat Zone Radii (Level1)")]
    private static void LoosenLevel1()
    {
        LoosenLevelFolder("Assets/_Project/ScriptableObjects/ItemSchemas/level1");
    }

    private static void LoosenLevelFolder(string folder)
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemSchemaSO", new[] { folder });

        int updatedZones = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemSchemaSO schema = AssetDatabase.LoadAssetAtPath<ItemSchemaSO>(path);
            if (schema == null || schema.heatZones == null) continue;

            foreach (var zone in schema.heatZones)
            {
                zone.innerRadius = 2f;
                zone.outerRadius = 5f;
                updatedZones++;
            }

            EditorUtility.SetDirty(schema);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[HeatZoneRadiusAdjuster] Updated {updatedZones} heat zones in {folder}.");
    }
}