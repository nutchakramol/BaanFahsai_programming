using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Attach to a UI prefab (Image + this script) representing one palette icon.
public class ItemPaletteSlot : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Image iconImage;

    private ItemSchemaSO _schema;
    private LevelController _controller;
    private Camera _mainCamera;
    private bool _isUsed;

    public void Setup(ItemSchemaSO schema, LevelController controller, Camera cam)
    {
        _schema = schema;
        _controller = controller;
        _mainCamera = cam;
        if (iconImage != null) iconImage.sprite = schema.icon;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isUsed || _schema.prefab == null) return;

        Vector3 spawnWorldPos = ScreenToWorld(eventData.position);
        GameObject instance = Instantiate(_schema.prefab, spawnWorldPos, Quaternion.identity);

        // Override the visual sprite so one shared prefab can represent
        // any item type — only the ScriptableObject's icon needs to differ.
        var sr = instance.GetComponent<SpriteRenderer>();
        if (sr != null && _schema.icon != null) sr.sprite = _schema.icon;

        DraggableItemView view = instance.GetComponent<DraggableItemView>();
        if (view == null)
        {
            Debug.LogError($"Prefab for '{_schema.itemId}' is missing DraggableItemView.");
            Destroy(instance);
            return;
        }

        string instanceId = System.Guid.NewGuid().ToString();
        view.Init(instanceId, _schema.itemId, _controller);
        view.OriginSlot = this;
        view.BeginExternalDrag();

        HideIcon();
    }

    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        Vector3 world = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
        world.z = 0f;
        return world;
    }

    public void HideIcon()
    {
        _isUsed = true;
        if (iconImage != null) iconImage.color = new Color(1f, 1f, 1f, 0.3f);
    }

    public void ShowIcon()
    {
        _isUsed = false;
        if (iconImage != null) iconImage.color = Color.white;
    }
}