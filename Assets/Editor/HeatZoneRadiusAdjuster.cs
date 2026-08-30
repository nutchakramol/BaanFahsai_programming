using UnityEngine;
using UnityEditor;

public static class HeatZoneRadiusAdjuster
{
    [MenuItem("Tools/Decor/Loosen All Heat Zone Radii (Level1)")]
    private static void LoosenRadii()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemSchemaSO", new[] { "Assets/_Project/ScriptableObjects/ItemSchemas/level1" });

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
        Debug.Log($"[HeatZoneRadiusAdjuster] Updated {updatedZones} heat zones across Level 1 items.");
    }
}