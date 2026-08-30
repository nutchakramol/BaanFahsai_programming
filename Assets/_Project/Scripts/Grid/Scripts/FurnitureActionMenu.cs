using UnityEngine;

public class FurnitureActionMenu : MonoBehaviour
{
    [Header("References")]
    public Canvas parentCanvas;

    [Tooltip(
        "Leave empty if Canvas is Screen Space - Overlay."
    )]
    public Camera uiCamera;

    [Header("Positioning")]
    public Vector3 worldOffset =
        new Vector3(0f, 1f, 0f);

    private RectTransform panelRect;
    private RectTransform canvasRect;
    private CanvasGroup canvasGroup;

    private GameObject lastSelectedFurniture;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        panelRect =
            GetComponent<RectTransform>();

        canvasGroup =
            GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                gameObject.AddComponent<CanvasGroup>();
        }

        if (parentCanvas != null)
        {
            canvasRect =
                parentCanvas.GetComponent<RectTransform>();
        }

        // IMPORTANT:
        // Do NOT SetActive(false)
        HideMenu();
    }

    private void LateUpdate()
    {
        if (PlacementController.Instance == null)
        {
            HideMenu();
            return;
        }

        GameObject selected =
            PlacementController.Instance.SelectedFurniture;

        // =====================================================
        // NOTHING SELECTED
        // =====================================================

        if (selected == null)
        {
            lastSelectedFurniture = null;
            HideMenu();
            return;
        }

        // =====================================================
        // FURNITURE SELECTED
        // =====================================================

        if (selected != lastSelectedFurniture)
        {
            lastSelectedFurniture =
                selected;

            ShowMenu();

            Debug.Log(
                $"[FurnitureActionMenu] Showing menu for " +
                $"{selected.name}"
            );
        }

        UpdatePosition(selected);
    }

    // =========================================================
    // SHOW
    // =========================================================

    private void ShowMenu()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 1f;

        canvasGroup.interactable = true;

        canvasGroup.blocksRaycasts = true;
    }

    // =========================================================
    // HIDE
    // =========================================================

    private void HideMenu()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;

        canvasGroup.interactable = false;

        canvasGroup.blocksRaycasts = false;
    }

    // =========================================================
    // POSITION
    // =========================================================

    private void UpdatePosition(
        GameObject furniture)
    {
        if (furniture == null)
            return;

        if (panelRect == null)
            return;

        if (parentCanvas == null)
            return;

        Camera worldCamera =
            Camera.main;

        if (worldCamera == null)
            return;

        Vector3 worldPosition =
            furniture.transform.position +
            worldOffset;

        Vector2 screenPosition =
            worldCamera.WorldToScreenPoint(
                worldPosition
            );

        // =====================================================
        // SCREEN SPACE OVERLAY
        // =====================================================

        if (parentCanvas.renderMode ==
            RenderMode.ScreenSpaceOverlay)
        {
            if (canvasRect == null)
            {
                canvasRect =
                    parentCanvas
                        .GetComponent<RectTransform>();
            }

            if (canvasRect == null)
                return;

            if (
                RectTransformUtility
                    .ScreenPointToLocalPointInRectangle(
                        canvasRect,
                        screenPosition,
                        null,
                        out Vector2 localPoint
                    )
            )
            {
                panelRect.anchoredPosition =
                    localPoint;
            }

            return;
        }

        // =====================================================
        // SCREEN SPACE CAMERA / WORLD SPACE
        // =====================================================

        Camera cameraToUse =
            uiCamera != null
                ? uiCamera
                : parentCanvas.worldCamera;

        if (canvasRect == null)
        {
            canvasRect =
                parentCanvas
                    .GetComponent<RectTransform>();
        }

        if (canvasRect == null)
            return;

        if (
            RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPosition,
                    cameraToUse,
                    out Vector2 cameraLocalPoint
                )
        )
        {
            panelRect.anchoredPosition =
                cameraLocalPoint;
        }
    }

    // =========================================================
    // DELETE
    // =========================================================

    public void OnDeleteButton()
    {
        if (PlacementController.Instance == null)
            return;

        PlacementController.Instance
            .OnDeleteButton();

        lastSelectedFurniture =
            null;

        HideMenu();
    }

    // =========================================================
    // FLIP
    // =========================================================

    public void OnFlipButton()
    {
        if (PlacementController.Instance == null)
            return;

        PlacementController.Instance
            .OnFlipButton();
    }

    // =========================================================
    // MOVE
    // =========================================================

    public void OnMoveUpButton()
    {
        if (PlacementController.Instance == null)
            return;

        PlacementController.Instance
            .OnMoveUpButton();
    }

    public void OnMoveDownButton()
    {
        if (PlacementController.Instance == null)
            return;

        PlacementController.Instance
            .OnMoveDownButton();
    }

    public void OnMoveLeftButton()
    {
        if (PlacementController.Instance == null)
            return;

        PlacementController.Instance
            .OnMoveLeftButton();
    }

    public void OnMoveRightButton()
    {
        if (PlacementController.Instance == null)
            return;

        PlacementController.Instance
            .OnMoveRightButton();
    }

    // =========================================================
    // MANUAL CLOSE
    // =========================================================

    public void CloseMenu()
    {
        lastSelectedFurniture =
            null;

        HideMenu();
    }
}