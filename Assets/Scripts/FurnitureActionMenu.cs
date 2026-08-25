using UnityEngine;

public class FurnitureActionMenu : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The parent Canvas this panel lives under")]
    public Canvas parentCanvas;

    [Tooltip("Optional: camera used by the Canvas if Render Mode is 'Screen Space - Camera' or 'World Space'. Leave empty if Canvas is 'Screen Space - Overlay'.")]
    public Camera uiCamera;

    [Header("Positioning")]
    [Tooltip("Offset above the furniture in world units, so the menu doesn't cover the piece")]
    public Vector3 worldOffset = new Vector3(0f, 1f, 0f);

    private RectTransform rect;
    private RectTransform canvasRect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (parentCanvas != null)
            canvasRect = parentCanvas.GetComponent<RectTransform>();

        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (PlacementController.Instance == null)
        {
            gameObject.SetActive(false);
            return;
        }

        GameObject selected = PlacementController.Instance.SelectedFurniture;

        if (selected == null)
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);
            return;
        }

        if (!gameObject.activeSelf) gameObject.SetActive(true);

        Vector3 worldPos = selected.transform.position + worldOffset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, uiCamera, out Vector2 localPoint);

        rect.anchoredPosition = localPoint;

        // TEMP DIAGNOSTIC
        Debug.Log($"WorldPos: {worldPos} | ScreenPos: {screenPos} | LocalPoint: {localPoint} | Screen: {Screen.width}x{Screen.height} | CanvasRect size: {canvasRect.rect.width}x{canvasRect.rect.height}");
    }
}