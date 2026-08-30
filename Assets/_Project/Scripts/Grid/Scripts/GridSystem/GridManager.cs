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

    // =========================================================
    // CELL KEY
    // =========================================================

    // Floor and Wall can have the same cell coordinate.
    // This makes them separate cells internally.
    private struct CellKey
    {
        public int surfaceId;
        public Vector3Int coordinate;

        public CellKey(Tilemap surface, Vector3Int coordinate)
        {
            surfaceId =
                surface != null
                    ? surface.GetInstanceID()
                    : 0;

            this.coordinate = coordinate;
        }
    }

    private readonly Dictionary<CellKey, GridCell> gridCells =
        new Dictionary<CellKey, GridCell>();

    // =========================================================
    // CURRENT SELECTION
    // =========================================================

    public GridCell SelectedCell { get; private set; }

    public Vector3Int SelectedCellCoord { get; private set; }

    public Tilemap SelectedSurface { get; private set; }

    public SurfaceBand SelectedSurfaceBand { get; private set; }

    // =========================================================
    // PREFERRED SURFACE
    // =========================================================

    // PlacementController tells us which surface the currently
    // selected furniture wants.
    //
    // Example:
    // bed    -> Floor
    // window -> Wall
    //
    // This solves Floor/Wall collider overlap.
    public SurfaceBand PreferredSurfaceBand
    {
        get;
        private set;
    } = SurfaceBand.Floor;

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
                "RoomManager should assign it."
            );
        }
    }

    private void Update()
    {
        HandleCellSelection();
    }

    // =========================================================
    // PREFERRED SURFACE
    // =========================================================

    public void SetPreferredSurfaceBand(
        SurfaceBand band)
    {
        PreferredSurfaceBand = band;
    }

    // =========================================================
    // CELL SELECTION
    // =========================================================

    private void HandleCellSelection()
    {
        if (MainCam == null)
        {
            MainCam = Camera.main;

            if (MainCam == null)
                return;
        }

        if (tilemapGrid == null)
            return;

        if (Mouse.current == null)
            return;

        // -----------------------------------------------------
        // Mouse screen -> world
        // -----------------------------------------------------

        Vector2 mousePos =
            Mouse.current.position.ReadValue();

        Vector3 screenPos =
            new Vector3(
                mousePos.x,
                mousePos.y,
                MainCam.nearClipPlane + 10f
            );

        Vector3 worldPos =
            MainCam.ScreenToWorldPoint(
                screenPos
            );

        worldPos.z = 0f;

        // -----------------------------------------------------
        // Find every collider under the pointer
        // -----------------------------------------------------

        RaycastHit2D[] hits =
            Physics2D.RaycastAll(
                worldPos,
                Vector2.zero
            );

        // -----------------------------------------------------
        // Fallback surface
        // -----------------------------------------------------
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log(
                $"========== SURFACE DEBUG ==========\n" +
                $"Mouse World = {worldPos}\n" +
                $"Preferred Surface = {PreferredSurfaceBand}\n" +
                $"Hit Count = {hits.Length}"
            );

            foreach (RaycastHit2D debugHit in hits)
            {
                if (debugHit.collider == null)
                    continue;

                Tilemap tm =
                    debugHit.collider.GetComponent<Tilemap>();

                if (tm == null)
                {
                    tm =
                        debugHit.collider
                            .GetComponentInParent<Tilemap>();
                }

                Debug.Log(
                    $"Collider = {debugHit.collider.name}, " +
                    $"Tilemap = {(tm != null ? tm.name : "NONE")}"
                );
            }
        }

        Tilemap fallbackTilemap = null;

        SurfaceBand fallbackBand =
            SurfaceBand.Floor;

        Vector3Int fallbackCoord =
            Vector3Int.zero;

        // -----------------------------------------------------
        // Preferred surface
        // -----------------------------------------------------

        Tilemap preferredTilemap = null;

        SurfaceBand preferredBand =
            PreferredSurfaceBand;

        Vector3Int preferredCoord =
            Vector3Int.zero;

        // =====================================================
        // SEARCH SURFACES
        // =====================================================

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            Tilemap hitTilemap =
                hit.collider.GetComponent<Tilemap>();

            if (hitTilemap == null)
            {
                hitTilemap =
                    hit.collider
                        .GetComponentInParent<Tilemap>();
            }

            if (hitTilemap == null)
                continue;

            if (surfaceTilemaps == null)
                continue;

            foreach (SurfaceTilemap surface
                     in surfaceTilemaps)
            {
                if (surface == null ||
                    surface.tilemap == null)
                {
                    continue;
                }

                if (surface.tilemap != hitTilemap)
                    continue;

                // ---------------------------------------------
                // Check actual tile
                // ---------------------------------------------

                Vector3Int tileCoord =
                    hitTilemap.WorldToCell(
                        worldPos
                    );

                // Collider may overlap this location even when
                // the Tilemap itself has no tile there.
                if (!hitTilemap.HasTile(tileCoord))
                    continue;

                // ---------------------------------------------
                // First valid result becomes fallback
                // ---------------------------------------------

                if (fallbackTilemap == null)
                {
                    fallbackTilemap =
                        hitTilemap;

                    fallbackBand =
                        surface.band;

                    fallbackCoord =
                        tileCoord;
                }

                // ---------------------------------------------
                // Furniture-required surface wins
                // ---------------------------------------------

                if (surface.band ==
                    PreferredSurfaceBand)
                {
                    preferredTilemap =
                        hitTilemap;

                    preferredBand =
                        surface.band;

                    preferredCoord =
                        tileCoord;

                    break;
                }
            }

            if (preferredTilemap != null)
                break;
        }

        // =====================================================
        // CHOOSE RESULT
        // =====================================================

        Tilemap selectedTilemap;

        SurfaceBand selectedBand;

        Vector3Int selectedCoord;

        if (preferredTilemap != null)
        {
            selectedTilemap =
                preferredTilemap;

            selectedBand =
                preferredBand;

            selectedCoord =
                preferredCoord;
        }
        else if (fallbackTilemap != null)
        {
            selectedTilemap =
                fallbackTilemap;

            selectedBand =
                fallbackBand;

            selectedCoord =
                fallbackCoord;
        }
        else
        {
            SelectedCell = null;
            SelectedSurface = null;

            return;
        }

        // =====================================================
        // SAVE SELECTION
        // =====================================================

        SelectedSurface =
            selectedTilemap;

        SelectedSurfaceBand =
            selectedBand;

        SelectedCellCoord =
            selectedCoord;

        SelectedCell =
            GetOrCreateCell(
                selectedCoord,
                selectedTilemap
            );

        // Uncomment only while debugging.
        /*
        Debug.Log(
            $"[GridManager] " +
            $"Preferred = {PreferredSurfaceBand}, " +
            $"Selected = {SelectedSurfaceBand}, " +
            $"Tilemap = {SelectedSurface.name}, " +
            $"Cell = {SelectedCellCoord}"
        );
        */
    }

    // =========================================================
    // GRID CELLS
    // =========================================================

    private GridCell GetOrCreateCell(
        Vector3Int cellCoord,
        Tilemap surface)
    {
        CellKey key =
            new CellKey(
                surface,
                cellCoord
            );

        if (!gridCells.TryGetValue(
                key,
                out GridCell cell))
        {
            Vector3 worldCenter;

            if (surface != null)
            {
                worldCenter =
                    surface.GetCellCenterWorld(
                        cellCoord
                    );
            }
            else
            {
                worldCenter =
                    tilemapGrid.GetCellCenterWorld(
                        cellCoord
                    );
            }

            cell =
                new GridCell(
                    new Vector2Int(
                        cellCoord.x,
                        cellCoord.y
                    ),
                    worldCenter
                );

            gridCells[key] =
                cell;
        }

        return cell;
    }

    // =========================================================
    // GET CELL
    // =========================================================

    public GridCell GetCell(
        Vector3Int coord)
    {
        if (SelectedSurface == null)
            return null;

        CellKey key =
            new CellKey(
                SelectedSurface,
                coord
            );

        gridCells.TryGetValue(
            key,
            out GridCell cell
        );

        return cell;
    }

    public GridCell GetCellByCoord2D(
        Vector2Int coord2D,
        int z = 0)
    {
        Vector3Int coord3D =
            new Vector3Int(
                coord2D.x,
                coord2D.y,
                z
            );

        return GetOrCreateCell(
            coord3D,
            SelectedSurface
        );
    }

    // =========================================================
    // OCCUPIED CELLS
    // =========================================================

    public List<GridCell> GetAllOccupiedCells()
    {
        List<GridCell> occupied =
            new List<GridCell>();

        foreach (GridCell cell
                 in gridCells.Values)
        {
            if (cell.IsOccupied)
            {
                occupied.Add(cell);
            }
        }

        return occupied;
    }

    // =========================================================
    // ROOM SWITCHING
    // =========================================================

    public void SetActiveRoom(
        Grid newGrid,
        SurfaceTilemap[] newSurfaces)
    {
        if (newGrid == null)
        {
            Debug.LogError(
                "[GridManager] SetActiveRoom received null Grid."
            );

            return;
        }

        tilemapGrid =
            newGrid;

        surfaceTilemaps =
            newSurfaces;

        // New room = new cached grid cells.
        gridCells.Clear();

        SelectedCell =
            null;

        SelectedSurface =
            null;

        Debug.Log(
            $"[GridManager] Active room changed. " +
            $"Grid: {newGrid.name}, " +
            $"Surfaces: " +
            $"{(newSurfaces != null ? newSurfaces.Length : 0)}"
        );
    }
}