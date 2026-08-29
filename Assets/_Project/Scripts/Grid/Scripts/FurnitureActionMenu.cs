using UnityEngine;

public class FurnitureActionMenu : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The parent Canvas this panel lives under")]
    public Canvas parentCanvas;

    [Tooltip(
        "Optional: camera used by the Canvas if Render Mode is " +
        "'Screen Space - Camera' or 'World Space'. " +
        "Leave empty if Canvas is 'Screen Space - Overlay'."
    )]
    public Camera uiCamera;

    [Header("Positioning")]
    [Tooltip(
        "Offset above the furniture in world units, " +
        "so the menu doesn't cover the piece"
    )]
    public Vector3 worldOffset = new Vector3(0f, 1f, 0f);

    private RectTransform rect;
    private RectTransform canvasRect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();

        if (parentCanvas != null)
        {
            canvasRect = parentCanvas.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError(
                "[FurnitureActionMenu] Parent Canvas is not assigned."
            );
        }

        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (PlacementController.Instance == null)
        {
            gameObject.SetActive(false);
            return;
        }

        GameObject selected =
            PlacementController.Instance.SelectedFurniture;

        if (selected == null)
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);

            return;
        }

        if (canvasRect == null)
            return;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        Vector3 worldPos =
            selected.transform.position + worldOffset;

        Camera worldCamera = Camera.main;

        if (worldCamera == null)
        {
            Debug.LogError(
                "[FurnitureActionMenu] No Main Camera found."
            );
            return;
        }

        Vector3 screenPos =
            worldCamera.WorldToScreenPoint(worldPos);

        // Screen Space - Overlay must use NULL camera.
        Camera conversionCamera = null;

        if (parentCanvas != null &&
            parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            conversionCamera = uiCamera != null
                ? uiCamera
                : parentCanvas.worldCamera;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                conversionCamera,
                out Vector2 localPoint))
        {
            rect.anchoredPosition = localPoint;
        }
    }
}