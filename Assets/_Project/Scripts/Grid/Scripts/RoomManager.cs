using UnityEngine;
using UnityEngine.Tilemaps;

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

        [Header("Grid Reference")]
        public Grid roomGrid;
        public GridManager.SurfaceTilemap[] surfaceTilemaps;
    }

    [Header("All Rooms")]
    public RoomData[] rooms;

    [Header("Starting Room")]
    public int startingRoomIndex = 0;

    private int currentRoomIndex = -1;

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
            Debug.LogError($"[RoomManager] Invalid room index: {index}");
            return;
        }

        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i] != null && rooms[i].roomRoot != null)
            {
                rooms[i].roomRoot.SetActive(false);
            }
        }

        RoomData selectedRoom = rooms[index];

        if (selectedRoom == null || selectedRoom.roomRoot == null)
        {
            Debug.LogError($"[RoomManager] Room {index} has no Room Root assigned.");
            return;
        }

        selectedRoom.roomRoot.SetActive(true);

        currentRoomIndex = index;

        if (PlacementController.Instance != null)
        {
            PlacementController.Instance.SetFurnitureSet(
                selectedRoom.furniturePrefabs
            );
        }
        else
        {
            Debug.LogError("[RoomManager] PlacementController.Instance not found.");
        }

        // NEW: switch GridManager to this room's own grid + tilemaps
        if (GridManager.Instance != null && selectedRoom.roomGrid != null)
        {
            GridManager.Instance.SetActiveRoom(selectedRoom.roomGrid, selectedRoom.surfaceTilemaps);
        }
        else
        {
            Debug.LogWarning($"[RoomManager] No grid/tilemap data configured for {selectedRoom.roomName}.");
        }

        Debug.Log(
            $"[RoomManager] Switched to {selectedRoom.roomName}, " +
            $"Furniture count: {selectedRoom.furniturePrefabs?.Length ?? 0}"
        );
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchRoom(0); // Bedroom
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchRoom(1); // Toilet
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchRoom(2); // LivingRoom
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchRoom(3); // Kitchen
        if (Input.GetKeyDown(KeyCode.Alpha5)) SwitchRoom(4); // Attic
    }
    public void ShowBedroom() { SwitchRoom(0); }
    public void ShowToilet() { SwitchRoom(1); }
    public void ShowAttic() { SwitchRoom(2); }
    public void ShowLivingRoom() { SwitchRoom(3); }
    public void ShowKitchen() { SwitchRoom(4); }
}