using UnityEngine;



public class RoomManager : MonoBehaviour
{
    [System.Serializable]
    public class RoomData
    {
        public string roomName;

        [Header("Room Root")]
        public GameObject roomRoot;

        [Header("Furniture Prefabs")]
        public GameObject[] furniturePrefabs;

        [Header("Placed Furniture Parent")]
        [Tooltip("Empty child such as Bedroom/PlacedFurniture")]
        public Transform placedFurnitureParent;

        [Header("Grid Reference")]
        public Grid roomGrid;

        public GridManager.SurfaceTilemap[] surfaceTilemaps;
    }
    [Header("Furniture Hotbar")]
    public FurnitureHotbarUI furnitureHotbarUI;

    [Header("All Rooms")]
    public RoomData[] rooms;

    [Header("Starting Room")]
    public int startingRoomIndex = 0;

    public int CurrentRoomIndex { get; private set; } = -1;

    private void Start()
    {
        SwitchRoom(startingRoomIndex);
    }

    public void SwitchRoom(int index)
    {
        if (rooms == null || rooms.Length == 0)
        {
            Debug.LogError("[RoomManager] No rooms configured.");
            return;
        }

        if (index < 0 || index >= rooms.Length)
        {
            Debug.LogError(
                $"[RoomManager] Invalid room index: {index}"
            );
            return;
        }


        // -------------------------------------------------
        // 1. Disable ALL rooms first
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
        // 2. Get selected room
        // -------------------------------------------------

        RoomData selectedRoom = rooms[index];

        if (selectedRoom == null ||
            selectedRoom.roomRoot == null)
        {
            Debug.LogError(
                $"[RoomManager] Room {index} has no Room Root assigned."
            );

            return;
        }
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
        // 3. Enable selected room
        // -------------------------------------------------

        selectedRoom.roomRoot.SetActive(true);

        CurrentRoomIndex = index;

        // -------------------------------------------------
        // 4. Change furniture list + parent
        // -------------------------------------------------

        if (PlacementController.Instance != null)
        {
            PlacementController.Instance.SetFurnitureSet(
                selectedRoom.furniturePrefabs
            );

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
        // 5. Switch GridManager to this room
        // -------------------------------------------------

        if (GridManager.Instance != null &&
            selectedRoom.roomGrid != null)
        {
            GridManager.Instance.SetActiveRoom(
                selectedRoom.roomGrid,
                selectedRoom.surfaceTilemaps
            );
        }
        else
        {
            Debug.LogWarning(
                $"[RoomManager] No grid/tilemap data configured for " +
                $"{selectedRoom.roomName}."
            );
        }

        // -------------------------------------------------
        // Debug
        // -------------------------------------------------

        Debug.Log(
            $"[RoomManager] Switched to {selectedRoom.roomName}, " +
            $"Furniture count: " +
            $"{(selectedRoom.furniturePrefabs != null ? selectedRoom.furniturePrefabs.Length : 0)}"
        );
    }

    // =====================================================
    // TEMPORARY KEYBOARD TESTING
    // =====================================================

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SwitchRoom(0); // Bedroom

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SwitchRoom(1); // Toilet

        if (Input.GetKeyDown(KeyCode.Alpha3))
            SwitchRoom(2); // Attic

        if (Input.GetKeyDown(KeyCode.Alpha4))
            SwitchRoom(3); // LivingRoom

        if (Input.GetKeyDown(KeyCode.Alpha5))
            SwitchRoom(4); // Kitchen
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

    public void ShowAttic()
    {
        SwitchRoom(2);
    }

    public void ShowLivingRoom()
    {
        SwitchRoom(3);
    }

    public void ShowKitchen()
    {
        SwitchRoom(4);
    }
}