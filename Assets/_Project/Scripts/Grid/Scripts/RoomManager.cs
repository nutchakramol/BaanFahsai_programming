using UnityEngine;



public class RoomManager : MonoBehaviour
{
    [System.Serializable]
    public class RoomData
    {
        public string roomName;
        public GameObject roomRoot;
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

    [Header("Rooms")]
    public RoomData[] rooms;

    [Header("Starting Room")]
    public int startingRoomIndex = 0;

    private int currentRoomIndex = -1;

    private void Start()
    {
        Debug.Log("[RoomManager] Start called");

        SwitchRoom(startingRoomIndex);
    }

    public void SwitchRoom(int index)
    {
        if (rooms == null || rooms.Length == 0)
        {
            Debug.LogError("[RoomManager] Rooms array is empty.");
            return;
        }

        if (index < 0 || index >= rooms.Length)
        {
            Debug.LogError(
                $"[RoomManager] Invalid room index: {index}"
            );
            return;
        }

        // 1. Turn OFF every room
        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i] != null && rooms[i].roomRoot != null)
            {
                rooms[i].roomRoot.SetActive(false);
            }
        }

        // 2. Get selected room
        RoomData selectedRoom = rooms[index];

        if (selectedRoom == null ||
            selectedRoom.roomRoot == null)
        {
            Debug.LogError($"[RoomManager] Room {index} has no Room Root assigned.");
            return;
        }

        // 3. Turn ON only selected room
        selectedRoom.roomRoot.SetActive(true);

        currentRoomIndex = index;

        // 4. Change furniture list
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
            Debug.LogError("[RoomManager] PlacementController.Instance not found.");
        }

        Debug.Log(
            $"[RoomManager] Switched to {selectedRoom.roomName}, " +
            $"Furniture count: {selectedRoom.furniturePrefabs?.Length ?? 0}"
        );
    }

    // Button methods
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