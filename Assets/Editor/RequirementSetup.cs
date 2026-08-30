using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class RequirementSetup
{
    private static void SetRequirements(string assetPath, List<LevelRequirement> requirements)
    {
        LevelDataSO data = AssetDatabase.LoadAssetAtPath<LevelDataSO>(assetPath);
        if (data == null)
        {
            Debug.LogError($"Could not load {assetPath}");
            return;
        }

        data.requirements = requirements;
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
        Debug.Log($"Set {requirements.Count} requirements on {assetPath}");
    }

    private static LevelRequirement Req(string id, ItemCategory category, int minCount, float minAvgScore)
    {
        return new LevelRequirement
        {
            requirementId = id,
            requiredCategory = category,
            minCount = minCount,
            minAvgScore = minAvgScore
        };
    }

    [MenuItem("Tools/Decor/Set Level 2 Requirements")]
    private static void SetLevel2()
    {
        SetRequirements("Assets/_Project/Scripts/Data/Level2Data.asset", new List<LevelRequirement>
        {
            Req("have sanitary", ItemCategory.Sanitary, 1, 0.3f),
            Req("have decor", ItemCategory.Decor, 1, 0.3f),
            Req("have lighting", ItemCategory.Lighting, 1, 0.3f),
            Req("have plant", ItemCategory.Plant, 1, 0.3f),
        });
    }

    [MenuItem("Tools/Decor/Set Level 3 Requirements")]
    private static void SetLevel3()
    {
        SetRequirements("Assets/_Project/Scripts/Data/Level3Data.asset", new List<LevelRequirement>
        {
            Req("have seating", ItemCategory.Seating, 1, 0.3f),
            Req("have electronics", ItemCategory.Electronics, 1, 0.3f),
            Req("have storage", ItemCategory.Storage, 1, 0.3f),
            Req("have plant", ItemCategory.Plant, 1, 0.3f),
            Req("have decor", ItemCategory.Decor, 1, 0.3f),
            Req("have rug", ItemCategory.Rug, 1, 0.3f),
            Req("have table", ItemCategory.Table, 1, 0.5f),
        });
    }

    [MenuItem("Tools/Decor/Set Level 4 Requirements")]
    private static void SetLevel4()
    {
        SetRequirements("Assets/_Project/Scripts/Data/Level4Data.asset", new List<LevelRequirement>
        {
            Req("have table", ItemCategory.Table, 1, 0.3f),
            Req("have appliance", ItemCategory.Appliance, 3, 0.3f),
            Req("have sanitary", ItemCategory.Sanitary, 1, 0.3f),
            Req("have storage", ItemCategory.Storage, 1, 0.3f),
        });
    }

    [MenuItem("Tools/Decor/Set Level 5 Requirements")]
    private static void SetLevel5()
    {
        SetRequirements("Assets/_Project/Scripts/Data/Level5Data.asset", new List<LevelRequirement>
        {
            Req("have lighting", ItemCategory.Lighting, 1, 0.3f),
            Req("have storage", ItemCategory.Storage, 2, 0.3f),
            Req("have decor", ItemCategory.Decor, 1, 0.3f),
            Req("have electronics", ItemCategory.Electronics, 1, 0.3f),
        });
    }
}