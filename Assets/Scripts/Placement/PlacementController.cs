using UnityEngine;

public class PlacementController : MonoBehaviour
{
    [Header("Furniture List")]
    [Tooltip("Drag your furniture prefabs here (each should have a SpriteRenderer)")]
    public GameObject[] furniturePrefabs;

    [Header("Placement Settings")]
    [Tooltip("Small vertical offset so the sprite doesn't Z-fight with the floor")]
    public float placementYOffset = 0.05f;

    [Header("Ghost Preview")]
    public GhostPreview ghostPreview;

    private int currentIndex = 0;

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

        if (cell == null) return;

        if (Input.GetMouseButtonDown(0)) // Left click: place
        {
            PlaceFurniture(cell);
        }
        else if (Input.GetMouseButtonDown(1)) // Right click: delete
        {
            DeleteFurniture(cell);
        }
        else if (Input.GetMouseButtonDown(2)) // Middle click: flip vertical
        {
            FlipFurnitureVertical(cell);
        }
    }

    private void DeleteFurniture(GridCell cell)
    {
        if (!cell.IsOccupied) return;

        Destroy(cell.OccupyingObject);
        cell.ClearOccupied();

        Debug.Log($"[PlacementController] Deleted furniture at {cell.Coordinate}");
    }

    private void FlipFurnitureVertical(GridCell cell)
    {
        if (!cell.IsOccupied) return;

        SpriteRenderer sr = cell.OccupyingObject.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        sr.flipX = !sr.flipX;

        Debug.Log($"[PlacementController] Flipped {cell.OccupyingObject.name} (flipX = {sr.flipX})");
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

            // 2D sprites don't need rotation to "lie flat" - they're already facing the camera.
            Quaternion spawnRot = Quaternion.identity;

            GameObject placed = Instantiate(prefab, spawnPos, spawnRot);
            cell.SetOccupied(placed);

            Debug.Log($"[PlacementController] Placed {prefab.name} at {cell.Coordinate}");
        }
}