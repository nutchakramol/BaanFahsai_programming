using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Reference")]
    public Grid tilemapGrid;

    [Header("Placeable Surfaces")]
    [Tooltip("Any Tilemap furniture can be placed on: Floor, WallLeft, WallRight")]
    public Tilemap[] placeableTilemaps;

    [Header("Selection Highlight")]
    [Tooltip("A simple Sprite GameObject that will move to follow the selected cell")]
    public GameObject highlightObject;

    private Camera mainCam;
    private Dictionary<Vector3Int, GridCell> gridCells = new Dictionary<Vector3Int, GridCell>();

    public GridCell SelectedCell { get; private set; }
    public Vector3Int SelectedCellCoord { get; private set; }
    public Tilemap SelectedSurface { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        mainCam = Camera.main;

        if (tilemapGrid == null)
            Debug.LogError("[GridManager] tilemapGrid is not assigned in the Inspector");

        // TEMP DIAGNOSTIC: prints the actual painted tile bounds for each tilemap
        if (placeableTilemaps != null)
        {
            foreach (var map in placeableTilemaps)
            {
                if (map == null) continue;
                map.CompressBounds();
                Debug.Log($"[{map.name}] cellBounds: {map.cellBounds} | tile count: {map.GetUsedTilesCount()}");
            }
        }
    }

    private void Update()
    {
        HandleCellSelection();
    }

    private void HandleCellSelection()
    {
        if (mainCam == null || tilemapGrid == null) return;
        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCam.nearClipPlane + 10f));
        worldPos.z = 0f;

        Vector3Int cellCoord = tilemapGrid.WorldToCell(worldPos);

        Tilemap hitSurface = null;
        if (placeableTilemaps != null)
        {
            foreach (var map in placeableTilemaps)
                {
                    if (map != null && map.HasTile(cellCoord))
                    {
                        hitSurface = map;
                        break;
                    }
                }
        }

        // TEMP DIAGNOSTIC: remove once selection works
        Debug.Log($"Mouse: {mousePos} | World: {worldPos} | Cell: {cellCoord} | Hit: {(hitSurface != null ? hitSurface.name : "none")}");

        if (hitSurface == null)
        {
            SelectedCell = null;
            SelectedSurface = null;
            return;
        }

        if (!gridCells.TryGetValue(cellCoord, out GridCell cell))
        {
            Vector3 cellWorldCenter = tilemapGrid.GetCellCenterWorld(cellCoord);
            cell = new GridCell(new Vector2Int(cellCoord.x, cellCoord.y), cellWorldCenter);
            gridCells[cellCoord] = cell;
        }

        SelectedCell = cell;
        SelectedCellCoord = cellCoord;
        SelectedSurface = hitSurface;
    }

    private void LateUpdate()
    {
        if (highlightObject == null) return;

        if (SelectedCell != null)
        {
            highlightObject.SetActive(true);
            highlightObject.transform.position = SelectedCell.WorldPosition + new Vector3(0f, 0.02f, 0f);
        }
        else
        {
            highlightObject.SetActive(false);
        }
    }

    public GridCell GetCell(Vector3Int coord)
    {
        gridCells.TryGetValue(coord, out GridCell cell);
        return cell;
    }
}