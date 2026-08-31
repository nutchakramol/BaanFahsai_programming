using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [System.Serializable]
    public class RoomData
    {
        [Header("Room")]
        public string roomName;
        public GameObject roomRoot;

        [Header("Furniture")]
        public GameObject[] furniturePrefabs;

        [Header("Placed Furniture Parent")]
        [Tooltip("Empty child such as Bedroom/PlacedFurniture")]
        public Transform placedFurnitureParent;

        [Header("Grid Reference")]
        public Grid roomGrid;

        public GridManager.SurfaceTilemap[] surfaceTilemaps;
    }


    // =====================================================
    // BACKGROUND MANAGER
    // =====================================================

    [Header("Background")]
    [SerializeField]
    private RoomBackgroundManager backgroundManager;

    // =====================================================
    // HOTBAR
    // =====================================================

    [Header("Furniture Hotbar")]
    public FurnitureHotbarUI furnitureHotbarUI;

    // =====================================================
    // ROOMS
    // =====================================================

    [Header("Rooms")]
    public RoomData[] rooms;

    [Header("Starting Room")]
    public int startingRoomIndex = 0;

    private int currentRoomIndex = -1;

    public int CurrentRoomIndex => currentRoomIndex;

    public RoomData CurrentRoom
    {
        get
        {
            if (rooms == null)
                return null;

            if (currentRoomIndex < 0 ||
                currentRoomIndex >= rooms.Length)
                return null;

            return rooms[currentRoomIndex];
        }
    }

    // =====================================================
    // START
    // =====================================================

private void Start()
{
    // Hide all rooms immediately, so no room flashes visible before
    // LevelSessionController.StartGameplay() picks the correct one
    // (which happens after NPC dialogue closes).
    for (int i = 0; i < rooms.Length; i++)
    {
        if (rooms[i] != null && rooms[i].roomRoot != null)
        {
            rooms[i].roomRoot.SetActive(false);
        }
    }
}
    // =====================================================
    // SWITCH ROOM
    // =====================================================

    public void SwitchRoom(int index)
    {
        // -------------------------------------------------
        // Validate rooms
        // -------------------------------------------------

        if (rooms == null || rooms.Length == 0)
        {
            Debug.LogError(
                "[RoomManager] Rooms array is empty."
            );
            return;
        }

        if (index < 0 || index >= rooms.Length)
        {
            Debug.LogError(
                $"[RoomManager] Invalid room index: {index}"
            );
            return;
        }

        RoomData selectedRoom = rooms[index];

        if (selectedRoom == null)
        {
            Debug.LogError(
                $"[RoomManager] Room {index} is null."
            );
            return;
        }

        if (selectedRoom.roomRoot == null)
        {
            Debug.LogError(
                $"[RoomManager] Room {index} " +
                $"({selectedRoom.roomName}) has no Room Root."
            );
            return;
        }

        // -------------------------------------------------
        // 1. Disable every room
        // -------------------------------------------------

        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i] != null &&
                rooms[i].roomRoot != null)
            {
                rooms[i].roomRoot.SetActive(false);
            }
        }

        // -------------------------------------------------
        // 2. Enable selected room
        // -------------------------------------------------

        selectedRoom.roomRoot.SetActive(true);

        currentRoomIndex = index;

        // -------------------------------------------------
        // 2b. Background
        // -------------------------------------------------

        if (backgroundManager != null)
        {
            backgroundManager.SetRoomBackground(selectedRoom.roomName);
        }
        else
        {
            Debug.LogWarning(
                "[RoomManager] BackgroundManager is not assigned."
            );
        }

        // -------------------------------------------------
        // 3. Placement Controller
        // -------------------------------------------------

        if (PlacementController.Instance != null)
        {
            // Give this room's furniture list
            PlacementController.Instance.SetFurnitureSet(
                selectedRoom.furniturePrefabs
            );

            // Make newly placed furniture become children
            // of this room's PlacedFurniture object.
            PlacementController.Instance.SetPlacedFurnitureParent(
                selectedRoom.placedFurnitureParent
            );
        }
        else
        {
            Debug.LogError(
                "[RoomManager] PlacementController.Instance not found."
            );
        }

        // -------------------------------------------------
        // 4. Furniture Hotbar
        // -------------------------------------------------

        if (furnitureHotbarUI != null)
        {
            furnitureHotbarUI.SetFurnitureList(
                selectedRoom.furniturePrefabs
            );
        }
        else
        {
            Debug.LogWarning(
                "[RoomManager] FurnitureHotbarUI is not assigned."
            );
        }

        // -------------------------------------------------
        // 5. Grid Manager
        // -------------------------------------------------

        if (GridManager.Instance != null)
        {
            if (selectedRoom.roomGrid != null)
            {
                GridManager.Instance.SetActiveRoom(
                    selectedRoom.roomGrid,
                    selectedRoom.surfaceTilemaps
                );
            }
            else
            {
                Debug.LogError(
                    $"[RoomManager] {selectedRoom.roomName} " +
                    "has no Room Grid assigned."
                );
            }
        }
        else
        {
            Debug.LogError(
                "[RoomManager] GridManager.Instance not found."
            );
        }

        // -------------------------------------------------
        // Debug
        // -------------------------------------------------

        Debug.Log(
            $"[RoomManager] Switched to {selectedRoom.roomName}. " +
            $"Index: {index}, " +
            $"Furniture: {selectedRoom.furniturePrefabs?.Length ?? 0}"
        );
    }

    // =====================================================
    // UI BUTTON METHODS
    // =====================================================

    public void ShowBedroom()
    {
        SwitchRoom(0);
    }

    public void ShowToilet()
    {
        SwitchRoom(1);
    }

    public void ShowLivingRoom()
    {
        SwitchRoom(2);
    }

    public void ShowKitchen()
    {
        SwitchRoom(3);
    }

    public void ShowAttic()
    {
        SwitchRoom(4);
    }
}