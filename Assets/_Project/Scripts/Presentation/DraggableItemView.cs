using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DraggableItemView : MonoBehaviour
{
    public string InstanceId { get; private set; }
    public string SchemaId;

    [HideInInspector] public ItemPaletteSlot OriginSlot; // set by the palette slot that spawned this

    private Camera _mainCamera;
    private Vector3 _dragOffset;
    private bool _isDragging;
    private LevelController _controller;
    private bool _hasBeenPlacedOnce;

    public void Init(string instanceId, string schemaId, LevelController controller)
    {
        InstanceId = instanceId;
        SchemaId = schemaId;
        _controller = controller;
        _mainCamera = Camera.main;
    }

    // Called by ItemPaletteSlot right after spawning, so it follows the
    // cursor immediately without needing a fresh OnMouseDown.
    public void BeginExternalDrag()
    {
        _dragOffset = Vector3.zero;
        _isDragging = true;
    }

    // Still supports re-picking-up an already-placed item to reposition it.
    private void OnMouseDown()
    {
        _dragOffset = transform.position - GetMouseWorldPos();
        _isDragging = true;
    }

    private void Update()
    {
        if (!_isDragging) return;

        transform.position = GetMouseWorldPos() + _dragOffset;

        if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
            ResolveDrop();
        }
    }

    private void ResolveDrop()
    {
        RoomView room = FindRoomUnderneath();

        if (room != null)
        {
            _controller.PlaceOrMoveItem(InstanceId, SchemaId, transform.position, room.RoomId);
            _hasBeenPlacedOnce = true;
        }
        else
        {
            ReturnToPalette();
        }
    }

    private void ReturnToPalette()
    {
        if (_hasBeenPlacedOnce)
            _controller.RemovePlacement(InstanceId);

        if (OriginSlot != null)
            OriginSlot.ShowIcon();

        Destroy(gameObject);
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