// ===================================================
// FILE: GridScoringBridge.cs
// Connects Plyfah's grid placement system to the existing
// LevelController/ScoringEngine, without modifying grid code.
// Reads level data dynamically via GameEvents.CurrentLevelData,
// so one shared scene works for any level (1-5).
// ===================================================
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridScoringBridge : MonoBehaviour
{
    [Header("Level Data (fallback for isolated testing only)")]
    [SerializeField] private LevelDataSO levelData;
    [SerializeField] private List<ItemSchemaSO> availableItems;

    [Header("Temporary — single room until full room data exists")]
    [SerializeField] private string defaultRoomId = "bedroom_1";

    private LevelController _controller;
    private readonly HashSet<Vector2Int> _knownPlacements = new HashSet<Vector2Int>();

    private void Start()
    {
        if (GameEvents.CurrentLevelData != null)
        {
            levelData = GameEvents.CurrentLevelData;
        }

        if (levelData == null)
        {
            Debug.LogError("[GridScoringBridge] No level data available.");
            return;
        }

        // Hide the dev room-switcher buttons during real gameplay —
        // room is locked to whichever room this level requires.
//        GameObject roomButtons = GameObject.Find("RoomButtons");
//        if (roomButtons != null)
//        {
//            roomButtons.SetActive(false);
//        }

        RoomManager roomManager = FindFirstObjectByType<RoomManager>();
        if (roomManager != null)
        {
            roomManager.SwitchRoom(levelData.levelIndex);
        }

        if (levelData.itemSchemas != null && levelData.itemSchemas.Count > 0)
        {
            availableItems = levelData.itemSchemas;
        }

        _controller = new LevelController(levelData, availableItems);
    }
        private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            CheckLevel();
        }

        if (GridManager.Instance == null || _controller == null) return;

        var occupiedCells = GridManager.Instance.GetAllOccupiedCells();
        foreach (var cell in occupiedCells)
        {
            if (_knownPlacements.Contains(cell.Coordinate)) continue;

            FurnitureItem furniture = cell.OccupyingObject.GetComponent<FurnitureItem>();
            if (furniture == null)
            {
                Debug.LogWarning($"[GridScoringBridge] Placed object at {cell.Coordinate} has no FurnitureItem component.");
                continue;
            }

            string schemaId = furniture.furnitureName;
            string instanceId = $"{cell.Coordinate.x}_{cell.Coordinate.y}";

            _controller.PlaceOrMoveItem(instanceId, schemaId, cell.WorldPosition, defaultRoomId);
            _knownPlacements.Add(cell.Coordinate);

            Debug.Log($"[GridScoringBridge] Reported placement: {schemaId} at {cell.Coordinate} in room {defaultRoomId}");
        }
    }

    public void CheckLevel()
    {
        if (_controller == null)
        {
            Debug.LogError("[GridScoringBridge] Controller is null — cannot check level.");
            return;
        }

        var result = _controller.CheckLevel();
        int stars = StarRatingCalculator.ComputeStars(result.OverallScorePercent, levelData.starThresholds);
        bool canProceed = result.OverallScorePercent >= levelData.minScoreToPass;

        Debug.Log($"[GridScoringBridge] Level checked — Overall: {result.OverallScorePercent}%, Stars: {stars}, Pass: {canProceed}");

        if (canProceed)
        {
            LevelProgress.UnlockUpTo(levelData.levelIndex + 1);
            Debug.Log($"[GridScoringBridge] Unlocked level {levelData.levelIndex + 1}");
        }

        MainMenuUI.ReturnToLevelSelectOnLoad = true;
        SceneManager.LoadScene("SampleScene");
    }
}