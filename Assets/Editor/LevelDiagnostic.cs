using UnityEngine;
using UnityEditor;

public static class LevelDiagnostic
{
    [MenuItem("Tools/Decor/Diagnose Levels 2-5")]
    private static void DiagnoseAllLevels()
    {
        DiagnoseLevel("Assets/_Project/Scripts/Data/Level2Data.asset", "level2");
        DiagnoseLevel("Assets/_Project/Scripts/Data/Level3Data.asset", "level3");
        DiagnoseLevel("Assets/_Project/Scripts/Data/Level4Data.asset", "level4");
        DiagnoseLevel("Assets/_Project/Scripts/Data/Level5Data.asset", "level5");
    }

    private static void DiagnoseLevel(string dataPath, string itemFolder)
    {
        LevelDataSO levelData = AssetDatabase.LoadAssetAtPath<LevelDataSO>(dataPath);
        if (levelData == null)
        {
            Debug.LogError($"Could not load {dataPath}");
            return;
        }

        Debug.Log($"===== {itemFolder.ToUpper()} REQUIREMENTS =====");
        foreach (var req in levelData.requirements)
        {
            Debug.Log($"[{itemFolder}] Requirement: '{req.requirementId}' — needs category {req.requiredCategory}, minCount {req.minCount}, minAvgScore {req.minAvgScore}");
        }

        Debug.Log($"===== {itemFolder.ToUpper()} ITEM SCHEMAS =====");
        string[] guids = AssetDatabase.FindAssets("t:ItemSchemaSO", new[] { $"Assets/_Project/ScriptableObjects/ItemSchemas/{itemFolder}" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemSchemaSO schema = AssetDatabase.LoadAssetAtPath<ItemSchemaSO>(path);
            if (schema == null) continue;

            Debug.Log($"[{itemFolder}] Item: '{schema.itemId}' — category: {schema.category}, heatZones: {schema.heatZones?.Count ?? 0}");
        }
    }
}