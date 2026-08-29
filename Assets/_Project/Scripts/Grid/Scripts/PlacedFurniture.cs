using UnityEngine;

// Auto-added to every placed furniture instance so it can be clicked
// directly and reliably matched back to its GridCell.
public class PlacedFurniture : MonoBehaviour
{
    public GridCell OwnerCell;
}