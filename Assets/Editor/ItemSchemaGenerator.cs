// ===================================================
// FILE: ItemSchemaGenerator.cs
// EDITOR-ONLY TOOL. Must live inside a folder named "Editor".
// Scans a Prefabs/Furniture/level{N} folder, matches each prefab
// to a sprite of the same name, and auto-creates an ItemSchemaSO
// asset for each match. Skips + logs anything it can't match.
// ===================================================
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public static class ItemSchemaGenerator
{
    // Adjust these two root paths if your folder names differ.
    private const string PrefabRoot = "Assets/_Project/Prefabs/Furniture";
    private const string SpriteRoot = "Assets/Sprites/Furniture";
    private const string OutputRoot = "Assets/_Project/ScriptableObjects/ItemSchemas";

    [MenuItem("Tools/Decor/Generate Item Schemas For Level 1")]
    private static void GenerateLevel1() => GenerateForLevel(1);

    [MenuItem("Tools/Decor/Generate Item Schemas For Level 2")]
    private static void GenerateLevel2() => GenerateForLevel(2);

    [MenuItem("Tools/Decor/Generate Item Schemas For Level 3")]
    private static void GenerateLevel3() => GenerateForLevel(3);

    [MenuItem("Tools/Decor/Generate Item Schemas For Level 4")]
    private static void GenerateLevel4() => GenerateForLevel(4);

    [MenuItem("Tools/Decor/Generate Item Schemas For Level 5")]
    private static void GenerateLevel5() => GenerateForLevel(5);

    private static void GenerateForLevel(int levelNumber)
    {
        string prefabFolder = $"{PrefabRoot}/level{levelNumber}";
        string spriteFolder = $"{SpriteRoot}/level{levelNumber}";
        string outputFolder = $"{OutputRoot}/level{levelNumber}";

        if (!AssetDatabase.IsValidFolder(prefabFolder))
        {
            Debug.LogError($"ItemSchemaGenerator: prefab folder not found: {prefabFolder}");
            return;
        }

        EnsureFolderExists(outputFolder);

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabFolder });

        int created = 0;
        int skipped = 0;
        var skippedNames = new List<string>();

        foreach (string guid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            string itemName = Path.GetFileNameWithoutExtension(prefabPath);

            string schemaPath = $"{outputFolder}/{itemName}.asset";
            if (AssetDatabase.LoadAssetAtPath<ItemSchemaSO>(schemaPath) != null)
            {
                Debug.Log($"ItemSchemaGenerator: '{itemName}' schema already exists, skipping.");
                continue;
            }

            Sprite matchedSprite = FindMatchingSprite(spriteFolder, itemName);
            if (matchedSprite == null)
            {
                skipped++;
                skippedNames.Add(itemName);
                continue; // don't create a half-filled asset for unmatched items
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            ItemSchemaSO schema = ScriptableObject.CreateInstance<ItemSchemaSO>();
            schema.itemId = itemName;
            schema.displayName = NicifyName(itemName);
            schema.icon = matchedSprite;
            schema.prefab = prefab;

            AssetDatabase.CreateAsset(schema, schemaPath);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"ItemSchemaGenerator: Level {levelNumber} done. Created: {created}, Skipped (no sprite match): {skipped}");

        if (skippedNames.Count > 0)
        {
            Debug.LogWarning($"ItemSchemaGenerator: These prefabs need MANUAL schema creation (no matching sprite found): {string.Join(", ", skippedNames)}");
        }
    }

    private static Sprite FindMatchingSprite(string spriteFolder, string itemName)
    {
        // Try common extensions since your project mixes .png and .jpg
        string[] extensions = { "png", "jpg", "jpeg" };

        foreach (string ext in extensions)
        {
            string candidatePath = $"{spriteFolder}/{itemName}.{ext}";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(candidatePath);
            if (sprite != null) return sprite;
        }

        return null;
    }

    private static string NicifyName(string rawName)
    {
        // "fish_tank" -> "Fish Tank", "bed2" -> "Bed2" (left as-is, edit manually if needed)
        string spaced = rawName.Replace("_", " ");
        return char.ToUpper(spaced[0]) + spaced.Substring(1);
    }

    private static void EnsureFolderExists(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath)) return;

        string[] parts = folderPath.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }
}