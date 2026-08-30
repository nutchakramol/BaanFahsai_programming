using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;

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

    [Header("Check / Result UI")]
    [SerializeField] private UnityEngine.UI.Button checkButton;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI percentText;
    [SerializeField] private UnityEngine.UI.Button retryButton;
    private LevelController _currentLevelController;
    private LevelDataSO _currentLevelData;

    private void Awake()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (checkButton != null)
        {
            checkButton.gameObject.SetActive(false); // NEW — hidden until gameplay starts
            checkButton.onClick.AddListener(CheckLevel);
        }

        if (retryButton != null)
            retryButton.onClick.AddListener(HandleRetry);
    }
    private void Start()
    {
        if (GameEvents.CurrentLevelData != null)
        {
            _currentLevelData = GameEvents.CurrentLevelData;

            if (npcDialogueUI != null)
                npcDialogueUI.Show(_currentLevelData, StartGameplay);
            else
                StartGameplay();
        }
        else if (defaultLevelData != null)
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
                "LevelSessionController: No current or default Level Data assigned."
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

        // Lock the grid/placement system to this level's room
        RoomManager roomManager = FindFirstObjectByType<RoomManager>();
        if (roomManager != null)
        {
            roomManager.SwitchRoom(_currentLevelData.levelIndex);
        }
        else
        {
            Debug.LogWarning("LevelSessionController: No RoomManager found in scene.");
        }

        if (gameplayUIDocument != null)
            gameplayUIDocument.gameObject.SetActive(true);

        if (checkButton != null)
            checkButton.gameObject.SetActive(true); // NEW — show Check button now that NPC dialogue is done


        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    // Call this from the Check button
    public void CheckLevel()
    {
        if (_currentLevelController == null)
        {
            Debug.LogError("LevelSessionController: No active level controller — nothing to check.");
            return;
        }

        _currentLevelController.CheckLevel();
    }

    private void HandleLevelChecked(
        int stars,
        float overallPercent,
        bool canProceed)
    {
        if (_currentLevelData == null) return;

        if (canProceed)
        {
            LevelProgress.CompleteLevel(_currentLevelData.levelIndex, stars);
        }
        else
        {
            LevelProgress.SaveStarsOnly(_currentLevelData.levelIndex, stars);
        }

        if (percentText != null)
            percentText.text = $"{overallPercent:F0}%";

        if (resultPanel != null)
            resultPanel.SetActive(true);
    }

    private void HandleRetry()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }
}