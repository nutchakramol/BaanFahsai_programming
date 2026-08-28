using UnityEngine;

public class PlacementController : MonoBehaviour
{
    public static PlacementController Instance { get; private set; }

    [Header("Furniture List")]
    [Tooltip("Drag your furniture prefabs here (each should have a SpriteRenderer)")]
    public GameObject[] furniturePrefabs;

    [Header("Placement Settings")]
    [Tooltip("Small vertical offset so the sprite doesn't Z-fight with the floor")]
    public float placementYOffset = 0.05f;

    [Header("Ghost Preview")]
    public GhostPreview ghostPreview;

    private int currentIndex = 0;

    private GameObject selectedFurniture;
    private GridCell selectedFurnitureCell;

    public GameObject SelectedFurniture => selectedFurniture;
    public GridCell SelectedFurnitureCell => selectedFurnitureCell;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        HandleCycling();
        HandlePlacementAndSelection();
        HandleDeselect();
    }

    // =========================================================
    // FURNITURE CYCLING
    // =========================================================

    private void HandleCycling()
    {
        if (furniturePrefabs == null || furniturePrefabs.Length == 0)
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            int direction = Input.GetKey(KeyCode.LeftShift) ? -1 : 1;
            int attempts = 0;

            do
            {
                currentIndex =
                    (currentIndex + direction + furniturePrefabs.Length)
                    % furniturePrefabs.Length;

                attempts++;
            }
            while (
                !IsFurnitureUnlocked(furniturePrefabs[currentIndex])
                && attempts < furniturePrefabs.Length
            );

            Debug.Log(
                $"[PlacementController] Selected furniture: " +
                $"{furniturePrefabs[currentIndex].name}"
            );
        }
    }

    private bool IsFurnitureUnlocked(GameObject prefab)
    {
        if (prefab == null)
            return false;

        FurnitureItem item = prefab.GetComponent<FurnitureItem>();

        if (item == null)
            return true;

        return item.IsUnlockedForPlayer();
    }

    // =========================================================
    // FOOTPRINT HELPERS
    // =========================================================

    private Vector2Int GetFootprintSize(GameObject prefab)
    {
        if (prefab == null) return Vector2Int.one;
        FurnitureItem item = prefab.GetComponent<FurnitureItem>();
        return item != null ? item.gridSize : Vector2Int.one;
    }

    private bool IsFootprintFree(Vector2Int origin, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int checkCoord = new Vector3Int(origin.x + x, origin.y + y, 0);
                GridCell checkCell = GridManager.Instance.GetCell(checkCoord);

                if (checkCell != null && checkCell.IsOccupied)
                    return false;
            }
        }
        return true;
    }

    private void OccupyFootprint(Vector2Int origin, Vector2Int size, GameObject placed)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int checkCoord = new Vector2Int(origin.x + x, origin.y + y);
                GridCell cell = GridManager.Instance.GetCellByCoord2D(checkCoord);
                cell.SetOccupied(placed);
            }
        }
    }

    private void ClearFootprint(Vector2Int origin, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int checkCoord = new Vector2Int(origin.x + x, origin.y + y);
                GridCell cell = GridManager.Instance.GetCellByCoord2D(checkCoord);
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
    
        // NEW: don't process world-space clicks if the pointer is over UI
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
    
        GridCell cell = GridManager.Instance.SelectedCell;

        if (ghostPreview != null)
        {
            GameObject previewPrefab = null;

            if (
                cell != null &&
                furniturePrefabs != null &&
                furniturePrefabs.Length > 0
            )
            {
                if (currentIndex < 0 || currentIndex >= furniturePrefabs.Length)
                {
                    currentIndex = 0;
                }

                previewPrefab = furniturePrefabs[currentIndex];
            }

            ghostPreview.UpdatePreview(previewPrefab, cell);
        }

        if (cell == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (cell.IsOccupied)
            {
                selectedFurniture = cell.OccupyingObject;
                selectedFurnitureCell = cell;

                Debug.Log(
                    $"[PlacementController] Selected placed furniture: " +
                    $"{selectedFurniture.name}"
                );
            }
            else
            {
                PlaceFurnitureAt(cell);
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            DeleteFurniture(cell);
        }
        else if (Input.GetMouseButtonDown(2))
        {
            FlipFurnitureHorizontal(cell);
        }
    }

    // =========================================================
    // DESELECT
    // =========================================================

    private void HandleDeselect()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearSelection();

            Debug.Log("[PlacementController] Selection cleared.");
        }
    }

    private void ClearSelection()
    {
        selectedFurniture = null;
        selectedFurnitureCell = null;
    }

    // =========================================================
    // PLACE FURNITURE
    // =========================================================

    private void PlaceFurnitureAt(GridCell cell)
    {
        if (cell == null)
            return;

        if (furniturePrefabs == null || furniturePrefabs.Length == 0)
        {
            Debug.LogWarning(
                "[PlacementController] No furniture prefabs assigned."
            );

            return;
        }

        if (currentIndex < 0 || currentIndex >= furniturePrefabs.Length)
        {
            currentIndex = 0;
        }

        GameObject prefab = furniturePrefabs[currentIndex];

        if (prefab == null)
        {
            Debug.LogError(
                $"[PlacementController] Furniture prefab at index " +
                $"{currentIndex} is null."
            );

            return;
        }

        // Full footprint check instead of single-cell check
        Vector2Int size = GetFootprintSize(prefab);
        if (!IsFootprintFree(cell.Coordinate, size))
        {
            Debug.Log(
                "[PlacementController] Footprint overlaps an occupied cell, can't place here."
            );

            return;
        }

        // NEW: surface band check — item must match the tilemap's surface type
        FurnitureItem furnitureCheck = prefab.GetComponent<FurnitureItem>();
        if (furnitureCheck != null && GridManager.Instance.SelectedSurfaceBand != furnitureCheck.surface)
        {
            Debug.Log(
                $"[PlacementController] {prefab.name} requires {furnitureCheck.surface} surface, " +
                $"but this cell is {GridManager.Instance.SelectedSurfaceBand}."
            );

            return;
        }

        if (!IsFurnitureUnlocked(prefab))
        {
            Debug.Log(
                $"[PlacementController] {prefab.name} is locked " +
                $"(requires a higher level)."
            );

            return;
        }

        Vector3 spawnPos =
            cell.WorldPosition +
            new Vector3(0f, placementYOffset, 0f);

        Quaternion spawnRot = Quaternion.identity;

        GameObject placed =
            Instantiate(prefab, spawnPos, spawnRot);

        OccupyFootprint(cell.Coordinate, size, placed);

        selectedFurniture = placed;
        selectedFurnitureCell = cell;

        Debug.Log(
            $"[PlacementController] Placed {prefab.name} " +
            $"at {cell.Coordinate}"
        );
    }

    // =========================================================
    // DELETE
    // =========================================================

    private void DeleteFurniture(GridCell cell)
    {
        if (cell == null)
            return;

        if (!cell.IsOccupied)
            return;

        if (selectedFurnitureCell == cell)
        {
            ClearSelection();
        }

        GameObject objectToDelete = cell.OccupyingObject;

        FurnitureItem item = objectToDelete != null ? objectToDelete.GetComponent<FurnitureItem>() : null;
        Vector2Int size = item != null ? item.gridSize : Vector2Int.one;
        ClearFootprint(cell.Coordinate, size);

        if (objectToDelete != null)
        {
            Destroy(objectToDelete);
        }

        Debug.Log(
            $"[PlacementController] Deleted furniture at {cell.Coordinate}"
        );
    }

    // =========================================================
    // FLIP
    // =========================================================

    private void FlipFurnitureHorizontal(GridCell cell)
    {
        if (cell == null || !cell.IsOccupied)
            return;

        GameObject furniture = cell.OccupyingObject;

        if (furniture == null)
            return;

        SpriteRenderer sr =
            furniture.GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            sr = furniture.GetComponentInChildren<SpriteRenderer>();
        }

        if (sr == null)
        {
            Debug.LogWarning(
                $"[PlacementController] No SpriteRenderer found on " +
                $"{furniture.name}"
            );

            return;
        }

        sr.flipX = !sr.flipX;

        Debug.Log(
            $"[PlacementController] Flipped {furniture.name} " +
            $"(flipX = {sr.flipX})"
        );
    }

    // =========================================================
    // MOVE
    // =========================================================

    public void MoveSelected(Vector2Int direction)
    {
        if (selectedFurniture == null || selectedFurnitureCell == null)
        {
            Debug.Log(
                "[PlacementController] No furniture selected to move."
            );

            return;
        }

        if (GridManager.Instance == null)
        {
            Debug.LogError(
                "[PlacementController] GridManager.Instance not found."
            );

            return;
        }

        Vector2Int targetCoord =
            selectedFurnitureCell.Coordinate + direction;

        GridCell targetCell =
            GridManager.Instance.GetCellByCoord2D(targetCoord);

        if (targetCell == null)
        {
            Debug.Log(
                "[PlacementController] Can't move there — invalid cell."
            );

            return;
        }

        FurnitureItem item = selectedFurniture.GetComponent<FurnitureItem>();
        Vector2Int size = item != null ? item.gridSize : Vector2Int.one;

        Vector2Int oldOrigin = selectedFurnitureCell.Coordinate;
        ClearFootprint(oldOrigin, size);

        if (!IsFootprintFree(targetCoord, size))
        {
            Debug.Log(
                "[PlacementController] Can't move there — footprint occupied."
            );

            OccupyFootprint(oldOrigin, size, selectedFurniture);
            return;
        }

        selectedFurniture.transform.position =
            targetCell.WorldPosition +
            new Vector3(0f, placementYOffset, 0f);

        OccupyFootprint(targetCoord, size, selectedFurniture);

        selectedFurnitureCell = targetCell;

        Debug.Log(
            $"[PlacementController] Moved furniture to {targetCoord}"
        );
    }

    // =========================================================
    // ROOM MANAGER SUPPORT
    // =========================================================

    public void SetFurnitureSet(GameObject[] newPrefabs)
    {
        furniturePrefabs = newPrefabs;

        currentIndex = 0;

        ClearSelection();

        int count =
            furniturePrefabs != null
                ? furniturePrefabs.Length
                : 0;

        Debug.Log(
            $"[PlacementController] Furniture set changed. Count: {count}"
        );

        if (count == 0)
        {
            Debug.LogWarning(
                "[PlacementController] Current room has no furniture prefabs assigned."
            );
        }
    }

    // =========================================================
    // UI BUTTON METHODS
    // =========================================================

    public void OnPlaceButton()
    {
        if (GridManager.Instance == null)
        {
            Debug.LogError(
                "[PlacementController] GridManager.Instance not found."
            );

            return;
        }

        GridCell cell =
            GridManager.Instance.SelectedCell;

        if (cell == null)
        {
            Debug.Log(
                "[PlacementController] No cell hovered to place into."
            );

            return;
        }

        PlaceFurnitureAt(cell);
    }

    public void OnDeleteButton()
    {
        if (
            selectedFurniture == null ||
            selectedFurnitureCell == null
        )
        {
            Debug.Log(
                "[PlacementController] No furniture selected to delete."
            );

            return;
        }

        DeleteFurniture(selectedFurnitureCell);
    }

    public void OnFlipButton()
    {
        if (
            selectedFurniture == null ||
            selectedFurnitureCell == null
        )
        {
            Debug.Log(
                "[PlacementController] No furniture selected to flip."
            );

            return;
        }

        FlipFurnitureHorizontal(selectedFurnitureCell);
    }

    public void OnMoveUpButton()
    {
        MoveSelected(new Vector2Int(0, 1));
    }

    public void OnMoveDownButton()
    {
        MoveSelected(new Vector2Int(0, -1));
    }

    public void OnMoveLeftButton()
    {
        MoveSelected(new Vector2Int(-1, 0));
    }

    public void OnMoveRightButton()
    {
        MoveSelected(new Vector2Int(1, 0));
    }
}