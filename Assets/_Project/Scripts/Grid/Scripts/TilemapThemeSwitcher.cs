using UnityEngine;
<<<<<<< HEAD
using UnityEngine.Tilemaps;

public class TilemapThemeSwitcher : MonoBehaviour
{
    [Header("Target Tilemaps")]
    public Tilemap floorTilemap;
    public Tilemap wallLeftTilemap;
    public Tilemap wallRightTilemap;

    // Wire a Floor swatch button's OnClick() to this, dragging in the
    // desired Tile asset as the method's parameter.
    public void SetFloorTile(TileBase newTile)
    {
        RepaintAll(floorTilemap, newTile);
    }

    // Wire a Wall swatch button's OnClick() to this the same way.
    // Repaints BOTH walls together so they stay matching.
    public void SetWallTile(TileBase newTile)
    {
        RepaintAll(wallLeftTilemap, newTile);
        RepaintAll(wallRightTilemap, newTile);
    }

    private void RepaintAll(Tilemap map, TileBase newTile)
    {
        if (map == null || newTile == null) return;

        map.CompressBounds();
        BoundsInt bounds = map.cellBounds;

        foreach (var pos in bounds.allPositionsWithin)
        {
            if (map.HasTile(pos))
            {
                map.SetTile(pos, newTile);
            }
        }

        Debug.Log($"[TilemapThemeSwitcher] Repainted {map.name} with {newTile.name}");
    }
}
=======

public class NewMonoBehaviourScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
>>>>>>> refs/remotes/origin/Gridsystem_pf
