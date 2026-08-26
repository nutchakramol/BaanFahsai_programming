using UnityEngine;

public class FurnitureActionMenu : MonoBehaviour
{
    [Header("References")]
    public Canvas parentCanvas;
    public Camera uiCamera;

    [Header("Positioning")]
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
    }
}