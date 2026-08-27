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

    // Public read-only access for UI scripts
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

    private void HandleCycling()
    {
        if (furniturePrefabs == null || furniturePrefabs.Length == 0) return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (Input.GetKey(KeyCode.LeftShift))
                currentIndex = (currentIndex - 1 + furniturePrefabs.Length) % furniturePrefabs.Length;
            else
                currentIndex = (currentIndex + 1) % furniturePrefabs.Length;

            Debug.Log($"[PlacementController] Selected furniture: {furniturePrefabs[currentIndex].name}");
        }
    }

    private void HandlePlacementAndSelection()
    {
        if (GridManager.Instance == null) return;

        GridCell cell = GridManager.Instance.SelectedCell;

        // Update Ghost Preview based on whether we are hovering over a cell
        if (ghostPreview != null)
        {
            ghostPreview.UpdatePreview(cell != null && !cell.IsOccupied ? furniturePrefabs[currentIndex] : null, cell);
        }

        if (cell == null) return;

        // Handle Mouse Clicks
        if (Input.GetMouseButtonDown(0)) // Left click: select existing or place new
        {
            if (cell.IsOccupied)
            {
                selectedFurniture = cell.OccupyingObject;
                selectedFurnitureCell = cell;
                Debug.Log($"[PlacementController] Selected placed furniture: {selectedFurniture.name}");
            }
            else
            {
                PlaceFurnitureAt(cell);
            }
        }
        else if (Input.GetMouseButtonDown(1)) // Right click: delete furniture
        {
            DeleteFurniture(cell);
        }
        else if (Input.GetMouseButtonDown(2)) // Middle click: flip horizontal directly
        {
            FlipFurnitureHorizontal(cell);
        }
    }

    private void HandleDeselect()
    {
        // Press Escape to clear the current selection
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            selectedFurniture = null;
            selectedFurnitureCell = null;
        }
    }

    private void PlaceFurnitureAt(GridCell cell)
    {
        if (furniturePrefabs == null || furniturePrefabs.Length == 0) return;

        // TODO: once CollisionChecker.cs is built, check multi-cell footprint here
        if (cell.IsOccupied)
        {
            Debug.Log("[PlacementController] Cell is already occupied, can't place here.");
            return;
        }

        GameObject prefab = furniturePrefabs[currentIndex];
        Vector3 spawnPos = cell.WorldPosition + new Vector3(0f, placementYOffset, 0f);
        Quaternion spawnRot = Quaternion.identity;

        GameObject placed = Instantiate(prefab, spawnPos, spawnRot);
        cell.SetOccupied(placed);

        // Auto-select the newly placed furniture so action menu/UI options trigger immediately
        selectedFurniture = placed;
        selectedFurnitureCell = cell;

        Debug.Log($"[PlacementController] Placed {prefab.name} at {cell.Coordinate}");
    }

    private void DeleteFurniture(GridCell cell)
    {
        if (!cell.IsOccupied) return;

        // If the deleted object was also our active selection, clear it
        if (selectedFurnitureCell == cell)
        {
            selectedFurniture = null;
            selectedFurnitureCell = null;
        }

        Destroy(cell.OccupyingObject);
        cell.ClearOccupied();

        Debug.Log($"[PlacementController] Deleted furniture at {cell.Coordinate}");
    }

    private void FlipFurnitureHorizontal(GridCell cell)
    {
        if (!cell.IsOccupied) return;

        SpriteRenderer sr = cell.OccupyingObject.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        sr.flipX = !sr.flipX;

        Debug.Log($"[PlacementController] Flipped {cell.OccupyingObject.name} (flipX = {sr.flipX})");
    }

    // ==========================================
    // UI BUTTON METHODS
    // ==========================================

    public void OnPlaceButton()
    {
        if (GridManager.Instance == null) return;
        GridCell cell = GridManager.Instance.SelectedCell;
        if (cell == null)
        {
            Debug.Log("[PlacementController] No cell hovered to place into.");
            return;
        }
        PlaceFurnitureAt(cell);
    }

    public void OnDeleteButton()
    {
        if (selectedFurniture == null || selectedFurnitureCell == null)
        {
            Debug.Log("[PlacementController] No furniture selected to delete.");
            return;
        }

        DeleteFurniture(selectedFurnitureCell);
    }

    public void OnFlipButton()
    {
        if (selectedFurniture == null)
        {
            Debug.Log("[PlacementController] No furniture selected to flip.");
            return;
        }

        FlipFurnitureHorizontal(selectedFurnitureCell);
    }

    public void MoveSelected(Vector2Int direction)
    {
        if (selectedFurniture == null || selectedFurnitureCell == null)
        {
            Debug.Log("[PlacementController] No furniture selected to move.");
            return;
        }

        Vector2Int targetCoord = selectedFurnitureCell.Coordinate + direction;
        GridCell targetCell = GridManager.Instance.GetCellByCoord2D(targetCoord);

        if (targetCell == null || targetCell.IsOccupied)
        {
            Debug.Log("[PlacementController] Can't move there — occupied or invalid.");
            return;
        }

        selectedFurnitureCell.ClearOccupied();
        selectedFurniture.transform.position = targetCell.WorldPosition + new Vector3(0f, placementYOffset, 0f);
        targetCell.SetOccupied(selectedFurniture);
        selectedFurnitureCell = targetCell;

        Debug.Log($"[PlacementController] Moved furniture to {targetCoord}");
    }

    public void OnMoveUpButton() => MoveSelected(new Vector2Int(0, 1));
    public void OnMoveDownButton() => MoveSelected(new Vector2Int(0, -1));
    public void OnMoveLeftButton() => MoveSelected(new Vector2Int(-1, 0));
    public void OnMoveRightButton() => MoveSelected(new Vector2Int(1, 0));
}