using System.Collections.Generic;
using UnityEngine;

public class GhostPreview : MonoBehaviour
{
    [Header("Furniture Sprite Preview")]
    [Tooltip("SpriteRenderer that shows the actual furniture art, semi-transparent")]
    public SpriteRenderer furnitureSpriteRenderer;

    [Header("Cell Highlight")]
    [Tooltip("A simple diamond-shaped GameObject prefab, same one used for your cell 'highlight' object")]
    public GameObject cellHighlightPrefab;

    [Header("Colors")]
    public Color validColor = new Color(1f, 1f, 1f, 0.6f);
    public Color blockedColor = new Color(1f, 0.3f, 0.3f, 0.6f);

    // Pool of highlight tiles so we're not Instantiating/Destroying every frame
    private List<SpriteRenderer> highlightPool = new List<SpriteRenderer>();

    private void Awake()
    {
        if (furnitureSpriteRenderer == null)
            Debug.LogWarning("[GhostPreview] furnitureSpriteRenderer not assigned in Inspector.");

        if (cellHighlightPrefab == null)
            Debug.LogWarning("[GhostPreview] cellHighlightPrefab not assigned in Inspector.");
    }

    /// <summary>
    /// Call every frame from PlacementController with the currently selected
    /// furniture prefab and the grid cell the mouse is over.
    /// </summary>
    public void UpdatePreview(GameObject furniturePrefab, GridCell originCell)
    {
        Debug.Log($"[GhostPreview] UpdatePreview called. Prefab: {furniturePrefab?.name}, Cell: {originCell?.Coordinate}");
    
        if (furniturePrefab == null || originCell == null)
        {
            Hide();
            return;
        }
    
        FurnitureItem item = furniturePrefab.GetComponent<FurnitureItem>();
        Vector2Int size = item != null ? item.gridSize : Vector2Int.one;
    
        bool footprintFree = CheckFootprintFree(originCell.Coordinate, size);
    
        bool surfaceMatches = true;
        if (item != null && GridManager.Instance != null)
        {
            surfaceMatches = GridManager.Instance.SelectedSurfaceBand == item.surface;
        }
    
        bool canPlace = footprintFree && surfaceMatches;
        Color previewColor = canPlace ? validColor : blockedColor;
    
        UpdateFurnitureSprite(furniturePrefab, originCell, previewColor);
        UpdateFootprintHighlights(originCell.Coordinate, size, previewColor);
    }
    private void UpdateFurnitureSprite(GameObject furniturePrefab, GridCell originCell, Color previewColor)
    {
        if (furnitureSpriteRenderer == null) return;

        SpriteRenderer prefabRenderer = furniturePrefab.GetComponent<SpriteRenderer>();
        if (prefabRenderer != null)
            furnitureSpriteRenderer.sprite = prefabRenderer.sprite;

        furnitureSpriteRenderer.gameObject.SetActive(true);
        furnitureSpriteRenderer.transform.position = originCell.WorldPosition;
        furnitureSpriteRenderer.color = previewColor;
    }

    private void UpdateFootprintHighlights(Vector2Int origin, Vector2Int size, Color previewColor)
    {
        if (cellHighlightPrefab == null || GridManager.Instance == null) return;

        int neededCount = size.x * size.y;
        EnsurePoolSize(neededCount);

        int index = 0;
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int checkCoord = new Vector3Int(origin.x + x, origin.y + y, 0);
                Vector3 worldPos = GridManager.Instance.tilemapGrid.GetCellCenterWorld(checkCoord);

                SpriteRenderer tile = highlightPool[index];
                tile.gameObject.SetActive(true);
                tile.transform.position = worldPos;
                tile.color = previewColor;

                index++;
            }
        }

        // Hide any pooled tiles we didn't need this frame
        for (int i = neededCount; i < highlightPool.Count; i++)
        {
            highlightPool[i].gameObject.SetActive(false);
        }
    }

    private void EnsurePoolSize(int needed)
    {
        while (highlightPool.Count < needed)
        {
            GameObject tile = Instantiate(cellHighlightPrefab, transform);
            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
            highlightPool.Add(sr);
        }
    }

    private bool CheckFootprintFree(Vector2Int origin, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int checkCoord = new Vector3Int(origin.x + x, origin.y + y, 0);
                GridCell checkCell = GridManager.Instance.GetCell(checkCoord);

                // Treat un-initialized cells as free; only block on confirmed occupied cells
                if (checkCell != null && checkCell.IsOccupied)
                    return false;
            }
        }
        return true;
    }

    public void Hide()
    {
        if (furnitureSpriteRenderer != null)
            furnitureSpriteRenderer.gameObject.SetActive(false);

        foreach (var tile in highlightPool)
        {
            tile.gameObject.SetActive(false);
        }
    }
}