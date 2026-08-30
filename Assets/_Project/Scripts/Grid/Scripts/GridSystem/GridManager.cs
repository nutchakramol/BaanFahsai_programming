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

    public Camera MainCam { get; private set; }

    private struct CellKey
    {
        public int surfaceId;
        public Vector3Int coord;

        public CellKey(Tilemap surface, Vector3Int coord)
        {
            surfaceId = surface != null ? surface.GetInstanceID() : 0;
            this.coord = coord;
        }
    }

    private readonly Dictionary<CellKey, GridCell> gridCells =
        new Dictionary<CellKey, GridCell>();

    public GridCell SelectedCell { get; private set; }
    public Vector3Int SelectedCellCoord { get; private set; }
    public Tilemap SelectedSurface { get; private set; }
    public SurfaceBand SelectedSurfaceBand { get; private set; }

    public SurfaceBand PreferredSurfaceBand { get; private set; } = SurfaceBand.Floor;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        MainCam = Camera.main;

        if (MainCam == null)
        {
            Debug.LogError(
                "[GridManager] Main Camera not found. " +
                "Make sure the gameplay camera has the MainCamera tag."
            );
        }

        if (tilemapGrid == null)
        {
            Debug.LogWarning(
                "[GridManager] tilemapGrid is not assigned yet. " +
                "RoomManager should assign it when the room starts."
            );
        }
    }

    private void Update()
    {
        HandleCellSelection();
    }

    public void SetPreferredSurfaceBand(SurfaceBand band)
    {
        PreferredSurfaceBand = band;
    }

    private void HandleCellSelection()
    {
        if (MainCam == null)
        {
            MainCam = Camera.main;
            if (MainCam == null)
                return;
        }

        if (tilemapGrid == null || Mouse.current == null)
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        Vector3 screenPos = new Vector3(
            mousePos.x,
            mousePos.y,
            MainCam.nearClipPlane + 10f
        );

        Vector3 worldPos = MainCam.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);

        Tilemap fallbackTilemap = null;
        SurfaceBand fallbackBand = SurfaceBand.Floor;
        Vector3Int fallbackCoord = Vector3Int.zero;

        Tilemap preferredTilemap = null;
        SurfaceBand preferredBand = PreferredSurfaceBand;
        Vector3Int preferredCoord = Vector3Int.zero;

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            Tilemap hitTilemap = hit.GetComponent<Tilemap>();

            if (hitTilemap == null)
                hitTilemap = hit.GetComponentInParent<Tilemap>();

            if (hitTilemap == null || surfaceTilemaps == null)
                continue;

            foreach (SurfaceTilemap surface in surfaceTilemaps)
            {
                if (surface == null || surface.tilemap == null)
                    continue;

                if (surface.tilemap != hitTilemap)
                    continue;

                Vector3Int tileCoord = hitTilemap.WorldToCell(worldPos);

                if (!hitTilemap.HasTile(tileCoord))
                    continue;

                if (fallbackTilemap == null)
                {
                    fallbackTilemap = hitTilemap;
                    fallbackBand = surface.band;
                    fallbackCoord = tileCoord;
                }

                if (surface.band == PreferredSurfaceBand)
                {
                    preferredTilemap = hitTilemap;
                    preferredBand = surface.band;
                    preferredCoord = tileCoord;
                    break;
                }
            }

            if (preferredTilemap != null)
                break;
        }

        Tilemap selectedTilemap;
        SurfaceBand selectedBand;
        Vector3Int selectedCoord;

        if (preferredTilemap != null)
        {
            selectedTilemap = preferredTilemap;
            selectedBand = preferredBand;
            selectedCoord = preferredCoord;
        }
        else if (fallbackTilemap != null)
        {
            selectedTilemap = fallbackTilemap;
            selectedBand = fallbackBand;
            selectedCoord = fallbackCoord;
        }
        else
        {
            SelectedCell = null;
            SelectedSurface = null;
            return;
        }

        SelectedSurface = selectedTilemap;
        SelectedSurfaceBand = selectedBand;
        SelectedCellCoord = selectedCoord;
        SelectedCell = GetOrCreateCell(selectedCoord, selectedTilemap);
    }

    private GridCell GetOrCreateCell(Vector3Int cellCoord, Tilemap surface)
    {
        CellKey key = new CellKey(surface, cellCoord);

        if (!gridCells.TryGetValue(key, out GridCell cell))
        {
            Vector3 worldCenter =
                surface != null
                    ? surface.GetCellCenterWorld(cellCoord)
                    : tilemapGrid.GetCellCenterWorld(cellCoord);

            cell = new GridCell(
                new Vector2Int(cellCoord.x, cellCoord.y),
                worldCenter
            );

            gridCells[key] = cell;
        }

        return cell;
    }

    public GridCell GetCell(Vector3Int coord)
    {
        if (SelectedSurface == null)
            return null;

        CellKey key = new CellKey(SelectedSurface, coord);
        gridCells.TryGetValue(key, out GridCell cell);
        return cell;
    }

    public GridCell GetCellByCoord2D(Vector2Int coord2D, int z = 0)
    {
        if (SelectedSurface == null)
            return null;

        Vector3Int coord3D = new Vector3Int(
            coord2D.x,
            coord2D.y,
            z
        );

        return GetOrCreateCell(coord3D, SelectedSurface);
    }

    public List<GridCell> GetAllOccupiedCells()
    {
        List<GridCell> occupied = new List<GridCell>();

        foreach (GridCell cell in gridCells.Values)
        {
            if (cell.IsOccupied)
                occupied.Add(cell);
        }

        return occupied;
    }

    public void SetActiveRoom(Grid newGrid, SurfaceTilemap[] newSurfaces)
    {
        if (newGrid == null)
        {
            Debug.LogError(
                "[GridManager] SetActiveRoom received a null Grid."
            );
            return;
        }

        tilemapGrid = newGrid;
        surfaceTilemaps = newSurfaces;

        gridCells.Clear();
        SelectedCell = null;
        SelectedSurface = null;

        Debug.Log(
            $"[GridManager] Active room changed. " +
            $"Grid: {newGrid.name}, " +
            $"Surfaces: {(newSurfaces != null ? newSurfaces.Length : 0)}"
        );
    }
}
