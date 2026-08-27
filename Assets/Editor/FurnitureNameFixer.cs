using UnityEngine;
using UnityEditor;
using System.IO;

public static class FurnitureNameFixer
{
    [MenuItem("Tools/Decor/Fix Empty Furniture Names")]
    private static void FixAllFurnitureNames()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Prefabs/Furniture" });

        int fixedCount = 0;
        int alreadyOkCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            FurnitureItem item = prefab.GetComponent<FurnitureItem>();
            if (item == null) continue;

            string expectedName = Path.GetFileNameWithoutExtension(path);

            if (string.IsNullOrEmpty(item.furnitureName))
            {
                item.furnitureName = expectedName;
                EditorUtility.SetDirty(prefab);
                fixedCount++;
                Debug.Log($"[FurnitureNameFixer] Fixed '{expectedName}'");
            }
            else
            {
                alreadyOkCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[FurnitureNameFixer] Done. Fixed: {fixedCount}, Already OK: {alreadyOkCount}");
    }
}