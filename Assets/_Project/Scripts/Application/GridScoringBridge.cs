// ===================================================
// FILE: GridScoringBridge.cs
// Connects Plyfah's grid placement system to the existing
// LevelController/ScoringEngine, without modifying grid code.
// Temporary single-room setup (bedroom_1) until Minny adds
// full room data for other levels.
// ===================================================
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridScoringBridge : MonoBehaviour
{
    [Header("Level Data")]
    [SerializeField] private LevelDataSO levelData;
    [SerializeField] private List<ItemSchemaSO> availableItems;

    [Header("Temporary — single room until full room data exists")]
    [SerializeField] private string defaultRoomId = "bedroom_1";

    private LevelController _controller;

    // Tracks which grid coordinates we've already reported as placed,
    // so we don't re-send the same item every frame.
    private readonly HashSet<Vector2Int> _knownPlacements = new HashSet<Vector2Int>();

    private void Start()
    {
        _controller = new LevelController(levelData, availableItems);
    }

    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) // temporary test key for Check Level
        {
            CheckLevel();
        }
        
        if (GridManager.Instance == null || _controller == null)
            {
                Debug.LogWarning($"[GridScoringBridge] Skipping — GridManager null: {GridManager.Instance == null}, Controller null: {_controller == null}");
                return;
            }

        var occupiedCells = GridManager.Instance.GetAllOccupiedCells();
        foreach (var cell in occupiedCells)
        {
            if (_knownPlacements.Contains(cell.Coordinate)) continue; // already reported

            FurnitureItem furniture = cell.OccupyingObject.GetComponent<FurnitureItem>();
            if (furniture == null)
            {
                Debug.LogWarning($"[GridScoringBridge] Placed object at {cell.Coordinate} has no FurnitureItem component.");
                continue;
            }

            string schemaId = furniture.furnitureName; // matches ItemSchemaSO.itemId
            string instanceId = $"{cell.Coordinate.x}_{cell.Coordinate.y}"; // stable per-cell ID

            _controller.PlaceOrMoveItem(instanceId, schemaId, cell.WorldPosition, defaultRoomId);
            _knownPlacements.Add(cell.Coordinate);

            Debug.Log($"[GridScoringBridge] Reported placement: {schemaId} at {cell.Coordinate} in room {defaultRoomId}");
        }
    }

    // Call this from a UI button to test scoring manually
    public void CheckLevel()
    {
        if (_controller == null) return;

        var result = _controller.CheckLevel();
        int stars = StarRatingCalculator.ComputeStars(result.OverallScorePercent, levelData.starThresholds);
        bool canProceed = result.OverallScorePercent >= levelData.minScoreToPass;

        Debug.Log($"[GridScoringBridge] Level checked — Overall: {result.OverallScorePercent}%, Stars: {stars}, Pass: {canProceed}");

        if (canProceed)
        {
            LevelProgress.UnlockUpTo(levelData.levelIndex + 1);
            Debug.Log($"[GridScoringBridge] Unlocked level {levelData.levelIndex + 1}");
        }
    }
    }