using UnityEngine;
using UnityEditor;

public static class AutoFillSchemas
{
    [MenuItem("Tools/Decor/Auto-Fill All Schemas on Selected LevelSessionController")]
    private static void FillSchemas()
    {
        LevelSessionController controller = Selection.activeGameObject?.GetComponent<LevelSessionController>();
        if (controller == null)
        {
            Debug.LogError("Select a GameObject with LevelSessionController first.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:ItemSchemaSO");
        var schemas = new System.Collections.Generic.List<ItemSchemaSO>();
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var schema = AssetDatabase.LoadAssetAtPath<ItemSchemaSO>(path);
            if (schema != null) schemas.Add(schema);
        }

        SerializedObject so = new SerializedObject(controller);
        SerializedProperty prop = so.FindProperty("allSchemas");
        prop.arraySize = schemas.Count;
        for (int i = 0; i < schemas.Count; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = schemas[i];
        so.ApplyModifiedProperties();

        Debug.Log($"Auto-filled {schemas.Count} schemas.");
    }
}