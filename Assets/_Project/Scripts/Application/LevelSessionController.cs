using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelSessionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<ItemSchemaSO> allSchemas;
    [SerializeField] private List<RoomView> roomViews;
    [SerializeField] private ItemPaletteView paletteView;
    [SerializeField] private UIDocument levelSelectUIDocument;
    [SerializeField] private UIDocument gameplayUIDocument;
    [SerializeField] private NPCDialogueUI npcDialogueUI;
    [SerializeField] private LevelDataSO defaultLevelData;
    private LevelController _currentLevelController;
    private LevelDataSO _currentLevelData;
    private void Start()
    {
        // Start with Level 1 when opening the scene directly.
        if (defaultLevelData != null)
        {
            _currentLevelData = defaultLevelData;

            if (npcDialogueUI != null)
                npcDialogueUI.Show(_currentLevelData, StartGameplay);
            else
                StartGameplay();
        }
        else
        {
            Debug.LogError(
                "LevelSessionController: Default Level Data is not assigned."
            );
        }
    }

    private void OnEnable()
    {
        GameEvents.OnLevelSelected += HandleLevelSelected;
        GameEvents.OnLevelChecked += HandleLevelChecked;
    }

    private void OnDisable()
    {
        GameEvents.OnLevelSelected -= HandleLevelSelected;
        GameEvents.OnLevelChecked -= HandleLevelChecked;
    }

    private void HandleLevelSelected(LevelDataSO levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("LevelSessionController: LevelData is null.");
            return;
        }

        // IMPORTANT:
        // This is the selected level's data.
        _currentLevelData = levelData;

        Debug.Log(
            "Selected Level: " +
            levelData.levelIndex +
            " | NPC: " +
            levelData.npcName
        );

        if (levelSelectUIDocument != null)
            levelSelectUIDocument.gameObject.SetActive(false);

        if (npcDialogueUI != null)
            npcDialogueUI.Show(levelData, StartGameplay);
        else
            StartGameplay();
    }

    private void StartGameplay()
    {
        if (_currentLevelData == null)
        {
            Debug.LogError(
                "LevelSessionController: Current Level Data is null."
            );
            return;
        }

        _currentLevelController =
            new LevelController(_currentLevelData, allSchemas);

        Dictionary<string, ItemSchemaSO> schemaLookup =
            allSchemas.ToDictionary(
                s => s.itemId,
                s => s
            );

        if (paletteView != null)
        {
            paletteView.BuildPalette(
                _currentLevelData,
                schemaLookup,
                _currentLevelController
            );
        }

        HashSet<string> activeRoomIds =
            new HashSet<string>(
                _currentLevelData.rooms.Select(r => r.roomId)
            );

        foreach (var roomView in roomViews)
        {
            if (roomView != null)
            {
                roomView.gameObject.SetActive(
                    activeRoomIds.Contains(roomView.RoomId)
                );
            }
        }

        if (gameplayUIDocument != null)
            gameplayUIDocument.gameObject.SetActive(true);
    }

    private void HandleLevelChecked(
        int stars,
        float overallPercent,
        bool canProceed)
    {
        if (canProceed && _currentLevelData != null)
        {
            LevelProgress.UnlockUpTo(
                _currentLevelData.levelIndex + 1
            );
        }
    }
}