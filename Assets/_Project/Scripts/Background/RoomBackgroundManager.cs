using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this to the BackgroundSprite GameObject (the one with the SpriteRenderer
/// that sits behind your isometric grid). Fill in "Room Backgrounds" in the Inspector
/// with one entry per room, then call SetRoomBackground("Bedroom") etc. whenever the
/// player switches rooms (from RoomButtons / _RoomManager).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class RoomBackgroundManager : MonoBehaviour
{
    [System.Serializable]
    public class RoomBackgroundEntry
    {
        public string roomName;   // Must match the room name string used elsewhere, e.g. "Bedroom"
        public Sprite sprite;
    }

    [Tooltip("One entry per room. roomName must exactly match the name your room-switch logic uses.")]
    [SerializeField] private List<RoomBackgroundEntry> roomBackgrounds = new List<RoomBackgroundEntry>();

    [Tooltip("If left empty, will use the SpriteRenderer on this GameObject.")]
    [SerializeField] private SpriteRenderer backgroundRenderer;

    private Dictionary<string, Sprite> _lookup;

    private void Awake()
    {
        if (backgroundRenderer == null)
            backgroundRenderer = GetComponent<SpriteRenderer>();

        _lookup = new Dictionary<string, Sprite>();
        foreach (var entry in roomBackgrounds)
        {
            if (string.IsNullOrEmpty(entry.roomName) || entry.sprite == null)
                continue;

            if (!_lookup.ContainsKey(entry.roomName))
                _lookup.Add(entry.roomName, entry.sprite);
        }
    }

    /// <summary>
    /// Call this from wherever room switching already happens
    /// (e.g. RoomButtons' onClick, or _RoomManager.OnRoomChanged).
    /// </summary>
    public void SetRoomBackground(string roomName)
    {
        if (_lookup.TryGetValue(roomName, out Sprite sprite))
        {
            backgroundRenderer.sprite = sprite;
        }
        else
        {
            Debug.LogWarning($"[RoomBackgroundManager] No background sprite assigned for room '{roomName}'.");
        }
    }
}
