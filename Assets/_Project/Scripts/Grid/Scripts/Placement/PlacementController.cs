using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class PlacementController : MonoBehaviour
{
    public static PlacementController Instance
    {
        get;
        private set;
    }

    [Header("Furniture List")]
    public GameObject[] furniturePrefabs;

    [Header("Placement Settings")]
    public float placementYOffset = 0.05f;

    [Header("Ghost Preview")]
    public GhostPreview ghostPreview;

    private int currentIndex = 0;

    private GameObject selectedFurniture;

    private GridCell selectedFurnitureCell;

    private Transform placedFurnitureParent;

    public GameObject SelectedFurniture =>
        selectedFurniture;

    public GridCell SelectedFurnitureCell =>
        selectedFurnitureCell;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        UpdatePreferredSurface();
    }

    private void Update()
    {
        HandleCycling();

        HandlePlacementAndSelection();

        HandleDeselect();
    }

    // =========================================================
    // FURNITURE SURFACE
    // =========================================================

    private void UpdatePreferredSurface()
    {
        if (GridManager.Instance == null)
            return;

        if (furniturePrefabs == null ||
            furniturePrefabs.Length == 0)
        {
            GridManager.Instance
                .SetPreferredSurfaceBand(
                    SurfaceBand.Floor
                );

            return;
        }

        if (currentIndex < 0 ||
            currentIndex >=
            furniturePrefabs.Length)
        {
            currentIndex = 0;
        }

        GameObject prefab =
            furniturePrefabs[currentIndex];

        if (prefab == null)
        {
            GridManager.Instance
                .SetPreferredSurfaceBand(
                    SurfaceBand.Floor
                );

            return;
        }

        FurnitureItem item =
            prefab.GetComponent<FurnitureItem>();

        SurfaceBand band =
            item != null
                ? item.surface
                : SurfaceBand.Floor;

        GridManager.Instance
            .SetPreferredSurfaceBand(
                band
            );
    }

    // =========================================================
    // CYCLING
    // =========================================================

    private void HandleCycling()
    {
        if (furniturePrefabs == null ||
            furniturePrefabs.Length == 0)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            int direction =
                Input.GetKey(KeyCode.LeftShift)
                    ? -1
                    : 1;

            int attempts = 0;

            do
            {
                currentIndex =
                    (
                        currentIndex +
                        direction +
                        furniturePrefabs.Length
                    )
                    % furniturePrefabs.Length;

                attempts++;
            }
            while (
                !IsFurnitureUnlocked(
                    furniturePrefabs[currentIndex]
                )
                &&
                attempts <
                furniturePrefabs.Length
            );

            UpdatePreferredSurface();

            Debug.Log(
                $"[PlacementController] Selected furniture: " +
                $"{furniturePrefabs[currentIndex].name}"
            );
        }
    }

    // =========================================================
    // LOCK
    // =========================================================

    private bool IsFurnitureUnlocked(
        GameObject prefab)
    {
        if (prefab == null)
            return false;

        FurnitureItem item =
            prefab.GetComponent<FurnitureItem>();

        if (item == null)
            return true;

        return item.IsUnlockedForPlayer();
    }

    // =========================================================
    // FOOTPRINT
    // =========================================================

    private Vector2Int GetFootprintSize(
        GameObject prefab)
    {
        if (prefab == null)
            return Vector2Int.one;

        FurnitureItem item =
            prefab.GetComponent<FurnitureItem>();

        if (item == null)
            return Vector2Int.one;

        Vector2Int size =
            item.gridSize;

        if (size.x <= 0)
            size.x = 1;

        if (size.y <= 0)
            size.y = 1;

        return size;
    }

    private bool IsFootprintFree(
        Vector2Int origin,
        Vector2Int size)
    {
        if (GridManager.Instance == null)
            return false;
        for (int x = 0;
             x < size.x;
             x++)
        {
            for (int y = 0;
                 y < size.y;
                 y++)
            {
                Vector3Int checkCoord =
                    new Vector3Int(
                        origin.x + x,
                        origin.y + y,
                        0
                    );
                GridCell checkCell =
                    GridManager.Instance
                        .GetCell(
                            checkCoord
                        );
                if (checkCell != null &&
                    checkCell.IsOccupied)
                {
                    return false;
                }
            }
        }
        return true;
        }
        private void OccupyFootprint(
        Vector2Int origin,
        Vector2Int size,
        GameObject placed)
    {
        for (int x = 0;
             x < size.x;
             x++)
        {
            for (int y = 0;
                 y < size.y;
                 y++)
            {
                Vector2Int checkCoord =
                    new Vector2Int(
                        origin.x + x,
                        origin.y + y
                    );

                GridCell cell =
                    GridManager.Instance
                        .GetCellByCoord2D(
                            checkCoord
                        );

                cell.SetOccupied(
                    placed
                );
            }
        }
    }

    private void ClearFootprint(
        Vector2Int origin,
        Vector2Int size)
    {
        for (int x = 0;
             x < size.x;
             x++)
        {
            for (int y = 0;
                 y < size.y;
                 y++)
            {
                Vector2Int checkCoord =
                    new Vector2Int(
                        origin.x + x,
                        origin.y + y
                    );

                GridCell cell =
                    GridManager.Instance
                        .GetCellByCoord2D(
                            checkCoord
                        );

                cell.ClearOccupied();
            }
        }
    }

    // =========================================================
    // PLACEMENT / SELECTION
    // =========================================================

    private void HandlePlacementAndSelection()
    {
        if (GridManager.Instance == null)
            return;

        GridCell cell =
            GridManager.Instance.SelectedCell;
        Debug.Log($"[PlacementController] cell: {cell?.Coordinate}, ghostPreview assigned: {ghostPreview != null}");

        // =====================================================
        // GHOST PREVIEW
        // =====================================================

        if (ghostPreview != null)
        {
            GameObject previewPrefab =
                null;

            if (cell != null &&
                furniturePrefabs != null &&
                furniturePrefabs.Length > 0)
            {
                if (currentIndex < 0 ||
                    currentIndex >=
                    furniturePrefabs.Length)
                {
                    currentIndex = 0;

                    UpdatePreferredSurface();
                }

                previewPrefab =
                    furniturePrefabs[currentIndex];
            }

            ghostPreview.UpdatePreview(
                previewPrefab,
                cell
            );
        }

        if (cell == null)
            return;

        // =====================================================
        // BLOCK CLICK THROUGH UI
        // =====================================================

        if (EventSystem.current != null &&
            EventSystem.current
                .IsPointerOverGameObject())
        {
            return;
        }

        // =====================================================
        // LEFT CLICK
        // =====================================================

        if (Input.GetMouseButtonDown(0))
        {
            if (cell.IsOccupied)
            {
                selectedFurniture =
                    cell.OccupyingObject;

                selectedFurnitureCell =
                    cell;

                if (selectedFurniture != null)
                {
                    Debug.Log(
                        $"[PlacementController] Selected: " +
                        $"{selectedFurniture.name}"
                    );
                }
            }
            else
            {
                PlaceFurnitureAt(
                    cell
                );
            }
        }

        // =====================================================
        // RIGHT CLICK
        // =====================================================

        else if (Input.GetMouseButtonDown(1))
        {
            DeleteFurniture(
                cell
            );
        }

        // =====================================================
        // MIDDLE CLICK
        // =====================================================

        else if (Input.GetMouseButtonDown(2))
        {
            FlipFurnitureHorizontal(
                cell
            );
        }
    }

    // =========================================================
    // DESELECT
    // =========================================================

    private void HandleDeselect()
    {
        if (Input.GetKeyDown(
                KeyCode.Escape))
        {
            ClearSelection();

            Debug.Log(
                "[PlacementController] Selection cleared."
            );
        }
    }

    private void ClearSelection()
    {
        selectedFurniture =
            null;

        selectedFurnitureCell =
            null;
    }

    // =========================================================
    // PLACE
    // =========================================================

    private void PlaceFurnitureAt(
        GridCell cell)
    {
        if (cell == null)
            return;

        if (GridManager.Instance == null)
            return;

        if (furniturePrefabs == null ||
            furniturePrefabs.Length == 0)
        {
            Debug.LogWarning(
                "[PlacementController] No furniture prefabs assigned."
            );

            return;
        }

        if (currentIndex < 0 ||
            currentIndex >=
            furniturePrefabs.Length)
        {
            currentIndex = 0;

            UpdatePreferredSurface();
        }

        GameObject prefab =
            furniturePrefabs[currentIndex];

        if (prefab == null)
        {
            Debug.LogError(
                $"[PlacementController] Furniture prefab " +
                $"at index {currentIndex} is null."
            );

            return;
        }

        // =====================================================
        // LOCK
        // =====================================================

        if (!IsFurnitureUnlocked(prefab))
        {
            Debug.Log(
                $"[PlacementController] {prefab.name} is locked."
            );

            return;
        }

        // =====================================================
        // SURFACE
        // =====================================================

        FurnitureItem furnitureItem =
            prefab.GetComponent<FurnitureItem>();

        if (furnitureItem != null)
        {
            if (GridManager.Instance
                    .SelectedSurfaceBand
                != furnitureItem.surface)
            {
                Debug.Log(
                    $"[PlacementController] {prefab.name} requires " +
                    $"{furnitureItem.surface}, " +
                    $"but selected surface is " +
                    $"{GridManager.Instance.SelectedSurfaceBand}."
                );

                return;
            }
        }

        // =====================================================
        // FOOTPRINT
        // =====================================================

        Vector2Int size =
            GetFootprintSize(
                prefab
            );

        if (!IsFootprintFree(
                cell.Coordinate,
                size))
        {
            Debug.Log(
                "[PlacementController] Can't place here. " +
                "Footprint overlaps occupied cells."
            );
            return;
        }
                // =====================================================
        // SPAWN
        // =====================================================

        // IMPORTANT:
        // cell.WorldPosition now comes from the selected Tilemap.
        //
        // Floor furniture -> Floor Tilemap cell center
        // Wall furniture  -> Wall Tilemap cell center
        Vector3 spawnPos =
            cell.WorldPosition +
            new Vector3(
                0f,
                placementYOffset,
                0f
            );

        GameObject placed;

        if (placedFurnitureParent != null)
        {
            placed =
                Instantiate(
                    prefab,
                    spawnPos,
                    Quaternion.identity,
                    placedFurnitureParent
                );
        }
        else
        {
            placed =
                Instantiate(
                    prefab,
                    spawnPos,
                    Quaternion.identity
                );

            Debug.LogWarning(
                "[PlacementController] PlacedFurniture parent is null. " +
                "Furniture created at scene root."
            );
        }

        OccupyFootprint(
            cell.Coordinate,
            size,
            placed
        );

        selectedFurniture =
            placed;

        selectedFurnitureCell =
            cell;

        Debug.Log(
            $"[PlacementController] Placed {prefab.name} " +
            $"on {GridManager.Instance.SelectedSurfaceBand} " +
            $"at {cell.Coordinate}"
        );
    }

    // =========================================================
    // DELETE
    // =========================================================

    private void DeleteFurniture(
        GridCell cell)
    {
        if (cell == null ||
            !cell.IsOccupied)
        {
            return;
        }

        GameObject objectToDelete =
            cell.OccupyingObject;

        if (objectToDelete == null)
        {
            cell.ClearOccupied();

            return;
        }

        FurnitureItem item =
            objectToDelete
                .GetComponent<FurnitureItem>();

        Vector2Int size =
            item != null
                ? item.gridSize
                : Vector2Int.one;

        Vector2Int origin =
            cell.Coordinate;

        if (objectToDelete ==
                selectedFurniture &&
            selectedFurnitureCell != null)
        {
            origin =
                selectedFurnitureCell.Coordinate;
        }

        ClearFootprint(
            origin,
            size
        );

        if (objectToDelete ==
            selectedFurniture)
        {
            ClearSelection();
        }

        Destroy(
            objectToDelete
        );

        Debug.Log(
            $"[PlacementController] Deleted furniture at {origin}"
        );
    }

    // =========================================================
    // FLIP
    // =========================================================

    private void FlipFurnitureHorizontal(
        GridCell cell)
    {
        if (cell == null ||
            !cell.IsOccupied)
        {
            return;
        }

        GameObject furniture =
            cell.OccupyingObject;

        if (furniture == null)
            return;

        SpriteRenderer sr =
            furniture
                .GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            sr =
                furniture
                    .GetComponentInChildren<SpriteRenderer>();
        }

        if (sr == null)
        {
            Debug.LogWarning(
                $"[PlacementController] No SpriteRenderer found on " +
                $"{furniture.name}"
            );

            return;
        }

        sr.flipX =
            !sr.flipX;

        Debug.Log(
            $"[PlacementController] Flipped {furniture.name}."
        );
    }

    // =========================================================
    // MOVE
    // =========================================================

    public void MoveSelected(
        Vector2Int direction)
    {
        if (selectedFurniture == null ||
            selectedFurnitureCell == null)
        {
            Debug.Log(
                "[PlacementController] No furniture selected."
            );

            return;
        }

        if (GridManager.Instance == null)
            return;

        Vector2Int oldOrigin =
            selectedFurnitureCell.Coordinate;

        Vector2Int targetCoord =
            oldOrigin +
            direction;

        GridCell targetCell =
            GridManager.Instance
                .GetCellByCoord2D(
                    targetCoord
                );

        if (targetCell == null)
            return;

        FurnitureItem item =
            selectedFurniture
                .GetComponent<FurnitureItem>();

        Vector2Int size =
            item != null
                ? item.gridSize
                : Vector2Int.one;

        // Temporarily free the old footprint.
        ClearFootprint(
            oldOrigin,
            size
        );

        if (!IsFootprintFree(
                targetCoord,
                size))
        {                        // Restore old occupancy.
            OccupyFootprint(
                oldOrigin,
                size,
                selectedFurniture
            );

            Debug.Log(
                "[PlacementController] Can't move there."
            );

            return;
        }

        selectedFurniture.transform.position =
            targetCell.WorldPosition +
            new Vector3(
                0f,
                placementYOffset,
                0f
            );

        OccupyFootprint(
            targetCoord,
            size,
            selectedFurniture
        );

        selectedFurnitureCell =
            targetCell;

        Debug.Log(
            $"[PlacementController] Moved furniture to {targetCoord}"
        );
    }

    // =========================================================
    // ROOM
    // =========================================================

    public void SetFurnitureSet(
        GameObject[] newPrefabs)
    {
        furniturePrefabs =
            newPrefabs;

        currentIndex =
            0;

        ClearSelection();

        // IMPORTANT:
        // Room changed, so update preferred Floor/Wall based
        // on the first furniture in the new room.
        UpdatePreferredSurface();

        int count =
            furniturePrefabs != null
                ? furniturePrefabs.Length
                : 0;

        Debug.Log(
            $"[PlacementController] Furniture set changed. " +
            $"Count: {count}"
        );
    }

    public void SetPlacedFurnitureParent(
        Transform parent)
    {
        placedFurnitureParent =
            parent;

        if (parent != null)
        {
            Debug.Log(
                $"[PlacementController] PlacedFurniture parent: " +
                $"{parent.name}"
            );
        }
    }

    // =========================================================
    // HOTBAR
    // =========================================================

    public void SelectFurniture(
        int index)
    {
        if (furniturePrefabs == null ||
            furniturePrefabs.Length == 0)
        {
            return;
        }

        if (index < 0 ||
            index >=
            furniturePrefabs.Length)
        {
            Debug.LogWarning(
                $"[PlacementController] Invalid furniture index: {index}"
            );

            return;
        }

        GameObject prefab =
            furniturePrefabs[index];

        if (prefab == null)
            return;

        if (!IsFurnitureUnlocked(
                prefab))
        {
            Debug.Log(
                $"[PlacementController] {prefab.name} is locked."
            );

            return;
        }

        currentIndex =
            index;

        // VERY IMPORTANT:
        // Window/light -> preferred Wall.
        // Bed/table    -> preferred Floor.
        UpdatePreferredSurface();

        FurnitureItem item =
            prefab.GetComponent<FurnitureItem>();

        Debug.Log(
            $"[PlacementController] Selected furniture: " +
            $"{prefab.name}, Surface: " +
            $"{(item != null ? item.surface.ToString() : "Floor")}"
        );
    }

    // =========================================================
    // UI ACTIONS
    // =========================================================

    public void OnPlaceButton()
    {
        if (GridManager.Instance == null)
            return;

        GridCell cell =
            GridManager.Instance.SelectedCell;

        if (cell == null)
        {
            Debug.Log(
                "[PlacementController] No cell selected."
            );

            return;
        }

        PlaceFurnitureAt(
            cell
        );
    }

    public void OnDeleteButton()
    {
        if (selectedFurniture == null ||
            selectedFurnitureCell == null)
        {
            return;
        }

        DeleteFurniture(
            selectedFurnitureCell
        );
    }

    public void OnFlipButton()
    {
        if (selectedFurniture == null ||
            selectedFurnitureCell == null)
        {
            return;
        }

        FlipFurnitureHorizontal(
            selectedFurnitureCell
        );
    }

    public void OnMoveUpButton()
    {
        MoveSelected(
            new Vector2Int(0, 1)
        );
    }

    public void OnMoveDownButton()
    {
        MoveSelected(
            new Vector2Int(0, -1)
        );
    }

    public void OnMoveLeftButton()
    {
        MoveSelected(
            new Vector2Int(-1, 0)
        );
    }

    public void OnMoveRightButton()
    {
        MoveSelected(
            new Vector2Int(1, 0)
        );
    }
}