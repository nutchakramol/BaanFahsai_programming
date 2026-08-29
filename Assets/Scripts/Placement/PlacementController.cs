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

    private GridCell hoveredOccupiedCell;

    public GridCell HoveredOccupiedCell => hoveredOccupiedCell;
    public GameObject SelectedFurniture => hoveredOccupiedCell != null ? hoveredOccupiedCell.OccupyingObject : null;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        HandleCycling();
        HandlePlacement();
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

    private void HandlePlacement()
    {
        if (furniturePrefabs == null || furniturePrefabs.Length == 0) return;
        if (GridManager.Instance == null) return;

        GridCell cell = GridManager.Instance.SelectedCell;

        if (ghostPreview != null)
            ghostPreview.UpdatePreview(cell != null ? furniturePrefabs[currentIndex] : null, cell);

        if (cell != null && cell.IsOccupied)
            hoveredOccupiedCell = cell;

        if (cell == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            PlaceFurniture(cell);
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

    private void PlaceFurniture(GridCell cell)
    {
        if (cell.IsOccupied)
        {
            Debug.Log("[PlacementController] Cell is already occupied, can't place here.");
            return;
        }

        GameObject prefab = furniturePrefabs[currentIndex];
        Vector3 spawnPos = cell.WorldPosition + new Vector3(0f, placementYOffset, 0f);
        GameObject placed = Instantiate(prefab, spawnPos, Quaternion.identity);
        cell.SetOccupied(placed);
        hoveredOccupiedCell = cell;

        Debug.Log($"[PlacementController] Placed {prefab.name} at {cell.Coordinate}");
    }

    private void DeleteFurniture(GridCell cell)
    {
        if (!cell.IsOccupied)
        {
            Debug.Log("[PlacementController] No furniture here to delete.");
            return;
        }

        Destroy(cell.OccupyingObject);
        cell.ClearOccupied();

        if (hoveredOccupiedCell == cell)
            hoveredOccupiedCell = null;

        Debug.Log($"[PlacementController] Deleted furniture at {cell.Coordinate}");
    }

    private void FlipFurnitureHorizontal(GridCell cell)
    {
        if (!cell.IsOccupied)
        {
            Debug.Log("[PlacementController] No furniture here to flip.");
            return;
        }

        SpriteRenderer sr = cell.OccupyingObject.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.Log("[PlacementController] Furniture has no SpriteRenderer to flip.");
            return;
        }

        sr.flipX = !sr.flipX;
        Debug.Log($"[PlacementController] Flipped furniture at {cell.Coordinate}, flipX={sr.flipX}");
    }

    public void OnPlaceButton()
    {
        if (GridManager.Instance == null) return;
        GridCell cell = GridManager.Instance.SelectedCell;
        if (cell == null)
        {
            Debug.Log("[PlacementController] No cell hovered to place into.");
            return;
        }
        PlaceFurniture(cell);
    }

    public void OnDeleteButton()
    {
        if (hoveredOccupiedCell == null)
        {
            Debug.Log("[PlacementController] No furniture selected to delete.");
            return;
        }
        DeleteFurniture(hoveredOccupiedCell);
    }

    public void OnFlipButton()
    {
        if (hoveredOccupiedCell == null)
        {
            Debug.Log("[PlacementController] No furniture selected to flip.");
            return;
        }
        FlipFurnitureHorizontal(hoveredOccupiedCell);
    }

    public void MoveSelected(Vector2Int direction)
    {
        if (hoveredOccupiedCell == null || !hoveredOccupiedCell.IsOccupied)
        {
            Debug.Log("[PlacementController] No furniture selected to move.");
            return;
        }

        Vector2Int targetCoord = hoveredOccupiedCell.Coordinate + direction;
        GridCell targetCell = GridManager.Instance.GetCellByCoord2D(targetCoord);

        if (targetCell == null || targetCell.IsOccupied)
        {
            Debug.Log("[PlacementController] Can't move there — occupied or invalid.");
            return;
        }

        GameObject furniture = hoveredOccupiedCell.OccupyingObject;
        hoveredOccupiedCell.ClearOccupied();

        furniture.transform.position = targetCell.WorldPosition + new Vector3(0f, placementYOffset, 0f);
        targetCell.SetOccupied(furniture);

        hoveredOccupiedCell = targetCell;

        Debug.Log($"[PlacementController] Moved furniture to {targetCoord}");
    }

    public void OnMoveUpButton() => MoveSelected(new Vector2Int(0, 1));
    public void OnMoveDownButton() => MoveSelected(new Vector2Int(0, -1));
    public void OnMoveLeftButton() => MoveSelected(new Vector2Int(-1, 0));
    public void OnMoveRightButton() => MoveSelected(new Vector2Int(1, 0));
}