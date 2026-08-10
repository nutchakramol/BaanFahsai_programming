// ===================================================
// FILE: DraggableItemView.cs
// Thin translation layer: mouse input -> LevelController calls.
// Contains NO scoring logic itself.
// ===================================================
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DraggableItemView : MonoBehaviour
{
    public string InstanceId { get; private set; }
    public string SchemaId;

    private Camera _mainCamera;
    private Vector3 _dragOffset;
    private bool _isDragging;
    private LevelController _controller;

    public void Init(string instanceId, string schemaId, LevelController controller)
    {
        InstanceId = instanceId;
        SchemaId = schemaId;
        _controller = controller;
        _mainCamera = Camera.main;
    }

    private void OnMouseDown()
    {
        _dragOffset = transform.position - GetMouseWorldPos();
        _isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (!_isDragging) return;
        transform.position = GetMouseWorldPos() + _dragOffset;
    }

    private void OnMouseUp()
    {
        _isDragging = false;
        RoomView room = FindRoomUnderneath();

        if (room != null)
        {
            _controller.PlaceOrMoveItem(InstanceId, SchemaId, transform.position, room.RoomId);
        }
        else
        {
            Debug.Log("Dropped outside a valid room zone.");
        }
    }

    private RoomView FindRoomUnderneath()
    {
        Collider2D hit = Physics2D.OverlapPoint(transform.position, LayerMask.GetMask("RoomZone"));
        return hit != null ? hit.GetComponent<RoomView>() : null;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = _mainCamera.WorldToScreenPoint(transform.position).z;
        return _mainCamera.ScreenToWorldPoint(mouseScreen);
    }
}