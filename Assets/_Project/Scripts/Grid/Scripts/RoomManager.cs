using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class RoomEntry
{
    [Tooltip("Must match the sceneName field in your LevelDataSO for this room")]
    public string roomName;

    [Tooltip("Parent GameObject holding this room's Grid, Floor, and Walls")]
    public GameObject roomRoot;

    public Grid roomGrid;
    public Tilemap floorTilemap;
    public Tilemap wallLeftTilemap;
    public Tilemap wallRightTilemap;

    [Tooltip("This room's furniture set, e.g. drag everything from Furniture/level3 for Living Room")]
    public GameObject[] furniturePrefabs;
}

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    [Header("All Rooms")]
    public RoomEntry[] rooms;

    [Header("Which room to show when the game starts")]
    public int startingRoomIndex = 0;

    private int currentRoomIndex = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        ShowRoom(startingRoomIndex);
    }

    // Called from LevelSelectUI instead of SceneManager.LoadScene
    public void ShowRoomByName(string roomName)
    {
        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i].roomName == roomName)
            {
                ShowRoom(i);
                return;
            }
        }
        Debug.LogError($"[RoomManager] No room found named '{roomName}'");
    }

    public void ShowRoom(int index)
    {
        if (index < 0 || index >= rooms.Length)
        {
            Debug.LogError($"[RoomManager] Room index {index} out of range.");
            return;
        }

        if (currentRoomIndex == index) return;

        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i].roomRoot != null)
                rooms[i].roomRoot.SetActive(false);
        }

        RoomEntry room = rooms[index];

        if (room.roomRoot != null)
            room.roomRoot.SetActive(true);

        currentRoomIndex = index;

        Tilemap[] surfaces = new Tilemap[] { room.floorTilemap, room.wallLeftTilemap, room.wallRightTilemap };
        if (GridManager.Instance != null)
            GridManager.Instance.SetActiveRoom(room.roomGrid, surfaces);

        if (PlacementController.Instance != null)
            PlacementController.Instance.SetFurnitureSet(room.furniturePrefabs);

        Debug.Log($"[RoomManager] Switched to room: {room.roomName}");
    }
}