using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [System.Serializable]
    public class RoomData
    {
        public string roomName;
        public GameObject roomRoot;
        public GameObject[] furniturePrefabs;
    }

    [Header("Rooms")]
    public RoomData[] rooms;

    [Header("Starting Room")]
    public int startingRoomIndex = 0;

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

        // Disable every room
        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i] == null)
                continue;

            if (rooms[i].roomRoot == null)
            {
                Debug.LogWarning(
                    $"[RoomManager] Room {i} has no Room Root."
                );
                continue;
            }

            rooms[i].roomRoot.SetActive(false);

            Debug.Log(
                $"[RoomManager] Disabled: {rooms[i].roomRoot.name}"
            );
        }

        RoomData selectedRoom = rooms[index];

        if (selectedRoom == null || selectedRoom.roomRoot == null)
        {
            Debug.LogError(
                $"[RoomManager] Selected room {index} has no Room Root."
            );
            return;
        }

        // Enable only selected room
        selectedRoom.roomRoot.SetActive(true);

        Debug.Log(
            $"[RoomManager] Enabled: {selectedRoom.roomRoot.name}"
        );

        // Update furniture
        if (PlacementController.Instance != null)
        {
            PlacementController.Instance.SetFurnitureSet(
                selectedRoom.furniturePrefabs
            );
        }
        else
        {
            Debug.LogWarning(
                "[RoomManager] PlacementController not found."
            );
        }
    }

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