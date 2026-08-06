using System.Collections.Generic;
using UnityEngine;

public class ItemPaletteView : MonoBehaviour
{
    [SerializeField] private GameObject paletteSlotPrefab; // UI prefab: Image + ItemPaletteSlot
    [SerializeField] private Transform slotParent;          // Horizontal Layout Group container

    public void BuildPalette(LevelDataSO levelData, Dictionary<string, ItemSchemaSO> schemaLookup, LevelController controller)
    {
        foreach (string schemaId in levelData.paletteItemIds)
        {
            if (!schemaLookup.TryGetValue(schemaId, out var schema))
            {
                Debug.LogWarning($"Palette references unknown schemaId: {schemaId}");
                continue;
            }

            GameObject slotGO = Instantiate(paletteSlotPrefab, slotParent);
            slotGO.GetComponent<ItemPaletteSlot>().Setup(schema, controller, Camera.main);
        }
    }
}