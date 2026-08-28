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

    private Dictionary<Vector3Int, GridCell> gridCells =
        new Dictionary<Vector3Int, GridCell>();

    public GridCell SelectedCell { get; private set; }

    public Vector3Int SelectedCellCoord { get; private set; }

    public Tilemap SelectedSurface { get; private set; }

    public SurfaceBand SelectedSurfaceBand { get; private set; }

    // =========================================================
    // UNITY
    // =========================================================

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
                "Make sure your gameplay camera has the MainCamera tag."
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

        Vector2 mousePos =
            Mouse.current.position.ReadValue();

        Vector3 screenPosition =
            new Vector3(
                mousePos.x,
                mousePos.y,
                MainCam.nearClipPlane + 10f
            );

        Vector3 worldPos =
            MainCam.ScreenToWorldPoint(screenPosition);

        worldPos.z = 0f;

        // Detect which Tilemap the mouse is currently over.
        RaycastHit2D hit =
            Physics2D.Raycast(
                worldPos,
                Vector2.zero
            );

        Tilemap hitSurface = null;

        SurfaceBand hitBand =
            SurfaceBand.Floor;

        if (hit.collider != null)
        {
            Tilemap hitTilemap =
                hit.collider.GetComponent<Tilemap>();

            // Sometimes collider can be on a child.
            if (hitTilemap == null)
            {
                hitTilemap =
                    hit.collider.GetComponentInParent<Tilemap>();
            }

            if (hitTilemap != null &&
                surfaceTilemaps != null)
            {
                foreach (SurfaceTilemap entry in surfaceTilemaps)
                {
                    if (entry == null)
                        continue;

                    if (entry.tilemap == hitTilemap)
                    {
                        hitSurface =
                            entry.tilemap;

                        hitBand =
                            entry.band;

                        break;
                    }
                }
            }
        }

        // Mouse is not over a registered placeable surface.
        if (hitSurface == null)
        {
            SelectedCell = null;
            SelectedSurface = null;

            return;
        }

        Vector3Int cellCoord =
            tilemapGrid.WorldToCell(worldPos);

        SelectedCell =
            GetOrCreateCell(cellCoord);

        SelectedCellCoord =
            cellCoord;

        SelectedSurface =
            hitSurface;

        SelectedSurfaceBand =
            hitBand;

        // Uncomment only when debugging.
        /*
        Debug.Log(
            $"[GridManager] Mouse world: {worldPos}, " +
            $"Cell coord: {cellCoord}, " +
            $"Surface: {hitBand}, " +
            $"Cell center: {SelectedCell.WorldPosition}"
        );
        */
    }

    // =========================================================
    // GRID CELLS
    // =========================================================

    private GridCell GetOrCreateCell(
        Vector3Int cellCoord)
    {
        if (!gridCells.TryGetValue(
                cellCoord,
                out GridCell cell))
        {
            Vector3 cellWorldCenter =
                tilemapGrid.GetCellCenterWorld(
                    cellCoord
                );

            cell =
                new GridCell(
                    new Vector2Int(
                        cellCoord.x,
                        cellCoord.y
                    ),
                    cellWorldCenter
                );

            gridCells[cellCoord] =
                cell;
        }

        return cell;
    }

    public GridCell GetCell(
        Vector3Int coord)
    {
        gridCells.TryGetValue(
            coord,
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
            coord3D
        );
    }

    // =========================================================
    // OCCUPIED CELLS
    // =========================================================

    public List<GridCell> GetAllOccupiedCells()
    {
        List<GridCell> occupied =
            new List<GridCell>();

        foreach (GridCell cell in gridCells.Values)
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
                "[GridManager] SetActiveRoom received a null Grid."
            );

            return;
        }

        tilemapGrid =
            newGrid;

        surfaceTilemaps =
            newSurfaces;

        // Each room gets its own cell cache.
        gridCells.Clear();

        SelectedCell =
            null;

        SelectedSurface =
            null;

        Debug.Log(
            $"[GridManager] Active room changed. " +
            $"Grid: {newGrid.name}, " +
            $"Surfaces: {(newSurfaces != null ? newSurfaces.Length : 0)}"
        );
    }
}