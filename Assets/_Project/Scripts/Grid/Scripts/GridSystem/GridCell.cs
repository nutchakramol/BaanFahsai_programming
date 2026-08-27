using UnityEngine;

[System.Serializable]
public class GridCell
{
    public Vector2Int Coordinate { get; private set; }
    public Vector3 WorldPosition { get; private set; }

    // Is this cell currently blocked by furniture?
    public bool IsOccupied { get; private set; }

    // Reference to whatever furniture object occupies this cell (null if empty)
    public GameObject OccupyingObject { get; private set; }

    public GridCell(Vector2Int coordinate, Vector3 worldPosition)
    {
        Coordinate = coordinate;
        WorldPosition = worldPosition;
        IsOccupied = false;
        OccupyingObject = null;
    }

    public void SetOccupied(GameObject obj)
    {
        IsOccupied = true;
        OccupyingObject = obj;
    }

    public void ClearOccupied()
    {
        IsOccupied = false;
        OccupyingObject = null;
    }
    
}