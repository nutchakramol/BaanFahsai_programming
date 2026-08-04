using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RoomView : MonoBehaviour
{
    public string RoomId;
    public RoomTag RoomTag;

    [Header("Visual (optional)")]
    [SerializeField] private Color roomTintColor = new Color(0.2f, 0.6f, 1f, 0.25f); // light blue, semi-transparent

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("RoomZone");
        GetComponent<Collider2D>().isTrigger = true;

        CreateVisualIfMissing();
    }

    // Auto-generates a flat colored quad matching the collider bounds,
    // so the room area is visible in Game view during Play, not just
    // as an invisible trigger collider.
    private void CreateVisualIfMissing()
    {
        if (transform.Find("RoomVisual") != null) return; // already created

        var col = GetComponent<BoxCollider2D>();
        if (col == null) return;

        GameObject visual = new GameObject("RoomVisual");
        visual.transform.SetParent(transform);
        visual.transform.localPosition = col.offset;
        visual.transform.localScale = new Vector3(col.size.x, col.size.y, 1f);

        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhiteSquareSprite();
        sr.color = roomTintColor;
        sr.sortingOrder = -10; // render behind items/furniture
    }

    private static Sprite _cachedSquareSprite;
    private static Sprite CreateWhiteSquareSprite()
    {
        // Generates a simple 1x1 white pixel sprite at runtime, so you
        // don't need to assign a sprite asset manually per room.
        if (_cachedSquareSprite != null) return _cachedSquareSprite;

        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        _cachedSquareSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return _cachedSquareSprite;
    }

    // Draws an outline in the SCENE view (Editor only) even without
    // pressing Play — helpful while you're positioning rooms and heat zones.
    private void OnDrawGizmos()
    {
        var col = GetComponent<BoxCollider2D>();
        if (col == null) return;

        Gizmos.color = Color.cyan;
        Vector3 center = transform.position + (Vector3)col.offset;
        Vector3 size = new Vector3(col.size.x, col.size.y, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}