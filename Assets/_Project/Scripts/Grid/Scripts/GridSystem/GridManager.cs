using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [System.Serializable]
    public class SurfaceTilemap
    {
        public Tilemap tilemap;
        public SurfaceBand band;
    }

    [Header("Grid Reference")]
    public Grid tilemapGrid;

    [Header("Placeable Surfaces")]
    public SurfaceTilemap[] surfaceTilemaps;

    [Header("Selection Highlight")]
    public GameObject highlightObject;

    public Camera MainCam { get; private set; }

    private Dictionary<Vector3Int, GridCell> gridCells = new Dictionary<Vector3Int, GridCell>();

    public GridCell SelectedCell { get; private set; }
    public Vector3Int SelectedCellCoord { get; private set; }
    public Tilemap SelectedSurface { get; private set; }
    public SurfaceBand SelectedSurfaceBand { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        MainCam = Camera.main;

        if (tilemapGrid == null)
            Debug.LogError("[GridManager] tilemapGrid is not assigned in the Inspector");
    }

    private void Update()
    {
        HandleCellSelection();
    }

    private void HandleCellSelection()
    {
        if (MainCam == null || tilemapGrid == null) return;
        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = MainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, MainCam.nearClipPlane + 10f));
        worldPos.z = 0f;

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        Tilemap hitSurface = null;
        SurfaceBand hitBand = SurfaceBand.Floor;

        if (hit.collider != null)
        {
            Tilemap hitTilemap = hit.collider.GetComponent<Tilemap>();
            if (hitTilemap != null && surfaceTilemaps != null)
            {
                foreach (var entry in surfaceTilemaps)
                {
                    if (entry.tilemap == hitTilemap)
                    {
                        hitSurface = entry.tilemap;
                        hitBand = entry.band;
                        break;
                    }
                }
            }
        }

        if (hitSurface == null)
        {
            SelectedCell = null;
            SelectedSurface = null;
            return;
        }

        Vector3Int cellCoord = tilemapGrid.WorldToCell(worldPos);
        SelectedCell = GetOrCreateCell(cellCoord);
        SelectedCellCoord = cellCoord;
        SelectedSurface = hitSurface;
        SelectedSurfaceBand = hitBand;
        Debug.Log($"[GridManager] Mouse world: {worldPos}, Cell coord: {cellCoord}, Cell center: {SelectedCell.WorldPosition}");
    }
    private GridCell GetOrCreateCell(Vector3Int cellCoord)
    {
        if (!gridCells.TryGetValue(cellCoord, out GridCell cell))
        {
            Vector3 cellWorldCenter = tilemapGrid.GetCellCenterWorld(cellCoord);
            cell = new GridCell(new Vector2Int(cellCoord.x, cellCoord.y), cellWorldCenter);
            gridCells[cellCoord] = cell;
        }
        return cell;
    }

    private void LateUpdate()
    {
        if (highlightObject == null) return;

        if (SelectedCell != null)
        {
            highlightObject.SetActive(true);
            highlightObject.transform.position = SelectedCell.WorldPosition;
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

    public GridCell GetCellByCoord2D(Vector2Int coord2D, int z = 0)
    {
        Vector3Int coord3D = new Vector3Int(coord2D.x, coord2D.y, z);
        return GetOrCreateCell(coord3D);
    }

    /// <summary>
    /// Returns every currently occupied cell, so scoring/evaluation code
    /// can inspect what's placed and where, without needing to track
    /// placement separately. Read-only snapshot — safe to call anytime.
    /// </summary>
    public List<GridCell> GetAllOccupiedCells()
    {
        List<GridCell> occupied = new List<GridCell>();
        foreach (var cell in gridCells.Values)
        {
            if (cell.IsOccupied)
                occupied.Add(cell);
        }
        return occupied;
    }

    public void SetActiveRoom(Grid newGrid, SurfaceTilemap[] newSurfaces)
    {
        tilemapGrid = newGrid;
        surfaceTilemaps = newSurfaces;
        gridCells.Clear();
        SelectedCell = null;
        SelectedSurface = null;
    }
}