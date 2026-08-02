// ===================================================
// FILE: LevelBootstrapper.cs
// Entry point for a level Scene. Constructs the LevelController,
// spawns item prefabs, and injects dependencies.
// This is the ONLY script allowed to "know about everything" —
// like a composition root / DI container entry point in backend code.
// ===================================================
using System.Collections.Generic;
using UnityEngine;

public class LevelBootstrapper : MonoBehaviour
{
    [Header("Level Data")]
    [SerializeField] private LevelDataSO levelData;
    [SerializeField] private List<ItemSchemaSO> availableItems;

    [Header("Spawn Settings")]
    [SerializeField] private GameObject itemPrefab; // for now, one prefab type (Sofa)
    [SerializeField] private Vector2 spawnPosition = new Vector2(-3f, 0f);

    private LevelController _controller;

    private void Start()
    {
        // Composition: wire the Application layer to the Data layer.
        _controller = new LevelController(levelData, availableItems);

        SpawnTestItem();
    }

    private void SpawnTestItem()
    {
        GameObject instance = Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
        DraggableItemView view = instance.GetComponent<DraggableItemView>();

        // Generate a unique instance id — GUID is fine for a small game.
        string instanceId = System.Guid.NewGuid().ToString();

        // Note: schemaId is already set in the Inspector on the prefab
        // (we set "sofa" earlier), so we read it back rather than
        // hardcoding it here — keeps this bootstrapper reusable.
        view.Init(instanceId, view.SchemaId, _controller);
    }
}