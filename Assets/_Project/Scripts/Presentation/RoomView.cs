// ===================================================
// FILE: RoomView.cs
// Marks a GameObject as a valid drop-zone for a room.
// The Collider2D here is a spatial "zone definition" —
// like a geofence: no physical collision, just region testing.
// ===================================================
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RoomView : MonoBehaviour
{
    public string RoomId;
    public RoomTag RoomTag;

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("RoomZone");
        GetComponent<Collider2D>().isTrigger = true;
    }
}