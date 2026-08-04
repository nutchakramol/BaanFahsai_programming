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

    // Remembers where this item was before the current drag started,
    // so we can snap back if the drop is invalid.
    private Vector3 _positionBeforeDrag;
    private bool _hasBeenPlacedSuccessfullyBefore;
    private Vector3 _lastValidPosition;

    public void Init(string instanceId, string schemaId, LevelController controller)
    {
        InstanceId = instanceId;
        SchemaId = schemaId;
        _controller = controller;
        _mainCamera = Camera.main;

        // Default fallback = wherever the item was spawned initially.
        _lastValidPosition = transform.position;
    }

    private void OnMouseDown()
    {
        _positionBeforeDrag = transform.position; // snapshot BEFORE this drag
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
            // Valid drop — commit it and remember this as the new fallback.
            _controller.PlaceOrMoveItem(InstanceId, SchemaId, transform.position, room.RoomId);
            _lastValidPosition = transform.position;
            _hasBeenPlacedSuccessfullyBefore = true;
        }
        else
        {
            Debug.Log("Dropped outside a valid room zone — snapping back.");

            // Snap back: if this item was already placed successfully once
            // before, return to that last valid spot. Otherwise return to
            // wherever it was before THIS drag attempt (usually spawn point).
            transform.position = _hasBeenPlacedSuccessfullyBefore
                ? _lastValidPosition
                : _positionBeforeDrag;
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