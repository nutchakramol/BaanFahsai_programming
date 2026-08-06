using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelBootstrapper : MonoBehaviour
{
    [Header("Level Data")]
    [SerializeField] private LevelDataSO levelData;
    [SerializeField] private List<ItemSchemaSO> availableItems;

    [Header("UI")]
    [SerializeField] private ItemPaletteView paletteView;
    [SerializeField] private LevelCompleteUI levelCompleteUI;

    private LevelController _controller;

    private void Start()
    {
        _controller = new LevelController(levelData, availableItems);

        var schemaLookup = availableItems.ToDictionary(s => s.itemId, s => s);
        paletteView.BuildPalette(levelData, schemaLookup, _controller);

        levelCompleteUI.Init(_controller);
    }
}