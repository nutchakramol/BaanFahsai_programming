using System.Collections.Generic;
using UnityEngine;

public class GhostPreview : MonoBehaviour
{
    [Header("Furniture Sprite Preview")]
    public SpriteRenderer furnitureSpritePreview;

    [Header("Cell Highlight")]
    public GameObject cellHighlightPrefab;

    [Header("Colors")]
    public Color validColor = new Color(0f, 1f, 0f, 0.8f);
    public Color blockedColor = new Color(1f, 0f, 0f, 0.8f);

    [Header("Rendering")]
    public string sortingLayerName = "highlight";
    public int highlightOrder = 100;
    public int furnitureOrder = 101;

    [Header("Position")]
    [Tooltip("Small Y offset so highlight renders slightly above floor")]
    public float highlightYOffset = 0.02f;

    private readonly List<GameObject> highlightPool =
        new List<GameObject>();

    // =========================================================
    // UPDATE PREVIEW
    // =========================================================

    public void UpdatePreview(GameObject prefab, GridCell cell)
    {
        HideAllHighlights();

        // Hide furniture ghost first
        if (furnitureSpritePreview != null)
        {
            furnitureSpritePreview.sprite = null;
        }

        if (prefab == null || cell == null)
            return;

        if (GridManager.Instance == null)
            return;

        // =====================================================
        // FIND FURNITURE DATA
        // =====================================================

        FurnitureItem item =
            prefab.GetComponent<FurnitureItem>();

        Vector2Int size =
            item != null
                ? item.gridSize
                : Vector2Int.one;

        size.x = Mathf.Max(1, size.x);
        size.y = Mathf.Max(1, size.y);

        // =====================================================
        // FURNITURE GHOST
        // =====================================================

        SpriteRenderer sourceRenderer =
            prefab.GetComponent<SpriteRenderer>();

        if (sourceRenderer == null)
        {
            sourceRenderer =
                prefab.GetComponentInChildren<SpriteRenderer>();
        }

        if (furnitureSpritePreview != null &&
            sourceRenderer != null)
        {
            furnitureSpritePreview.sprite =
                sourceRenderer.sprite;

            furnitureSpritePreview.transform.position =
                cell.WorldPosition +
                new Vector3(0f, 0.05f, 0f);

            furnitureSpritePreview.flipX =
                sourceRenderer.flipX;

            furnitureSpritePreview.sortingLayerName =
                sortingLayerName;

            furnitureSpritePreview.sortingOrder =
                furnitureOrder;

            furnitureSpritePreview.enabled = true;
        }

        // =====================================================
        // CHECK VALIDITY
        // =====================================================

        bool valid = true;

        // Correct surface?
        if (item != null)
        {
            if (GridManager.Instance.SelectedSurfaceBand
                != item.surface)
            {
                valid = false;
            }
        }

        List<GridCell> footprintCells =
            new List<GridCell>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int coord =
                    cell.Coordinate +
                    new Vector2Int(x, y);

                GridCell target =
                    GridManager.Instance.GetCellByCoord2D(coord);

                footprintCells.Add(target);

                if (target == null ||
                    target.IsOccupied)
                {
                    valid = false;
                }
            }
        }

        // =====================================================
        // SHOW HIGHLIGHTS
        // =====================================================

        for (int i = 0; i < footprintCells.Count; i++)
        {
            GridCell target =
                footprintCells[i];

            if (target == null)
                continue;

            GameObject highlight =
                GetHighlight(i);

            if (highlight == null)
                continue;

            highlight.SetActive(true);

            highlight.transform.position =
                target.WorldPosition +
                new Vector3(
                    0f,
                    highlightYOffset,
                    -0.1f
                );

            highlight.transform.rotation =
                Quaternion.identity;

            SpriteRenderer sr =
                highlight.GetComponent<SpriteRenderer>();

            if (sr == null)
            {
                sr =
                    highlight.GetComponentInChildren<SpriteRenderer>();
            }

            if (sr != null)
            {
                sr.color =
                    valid
                        ? validColor
                        : blockedColor;

                sr.sortingLayerName =
                    sortingLayerName;

                sr.sortingOrder =
                    highlightOrder;

                sr.enabled = true;
            }
            
        }
    }

    // =========================================================
    // HIGHLIGHT POOL
    // =========================================================

    private GameObject GetHighlight(int index)
    {
        if (cellHighlightPrefab == null)
        {
            Debug.LogError(
                "[GhostPreview] Cell Highlight Prefab is not assigned."
            );

            return null;
        }

        // Reuse existing object
        if (index < highlightPool.Count)
        {
            return highlightPool[index];
        }

        // Create a new one only when needed
        GameObject newHighlight =
            Instantiate(cellHighlightPrefab);

        newHighlight.name =
            $"GhostHighlight_{highlightPool.Count}";

        // IMPORTANT:
        // make sure prefab's saved Transform does not affect runtime
        newHighlight.transform.position =
            Vector3.zero;

        newHighlight.transform.rotation =
            Quaternion.identity;

        newHighlight.transform.localScale =
            Vector3.one;

        highlightPool.Add(newHighlight);

        return newHighlight;
    }

    private void HideAllHighlights()
    {
        foreach (GameObject highlight in highlightPool)
        {
            if (highlight != null)
            {
                highlight.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        HideAllHighlights();

        if (furnitureSpritePreview != null)
        {
            furnitureSpritePreview.sprite = null;
        }
    }

    private void OnDestroy()
    {
        foreach (GameObject highlight in highlightPool)
        {
            if (highlight != null)
            {
                Destroy(highlight);
            }
        }

        highlightPool.Clear();
    }
}