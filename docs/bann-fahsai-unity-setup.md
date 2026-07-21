# บ้านฟ้าใส (Bann Fahsai) — Unity Project Setup Guide

Architecture target: Unity 2D, side-view/dollhouse perspective, X/Y grid placement, ScriptableObject-driven data, JSON save system, 4-person team split.

---

## 0. Prerequisites

- **Unity Hub** installed
- **Unity 6000.0 LTS** (latest LTS at time of writing — check Unity Hub for the current LTS patch and use that; avoid Tech Stream/beta versions for a team project)
- **Git** + a shared remote (GitHub/GitLab) — required day one since 4 people are touching the same project

---

## 1. Create the Project

1. Unity Hub → **New Project**
2. Template: **2D (URP)** — gives you the 2D Universal Render Pipeline preconfigured (better lighting/post-processing later if you want mood lighting per theme — relevant since your base anchor items are windows + light bulbs).
3. Project name: `BannFahsai`
4. Confirm project location is inside your Git repo folder (or init Git right after).

### Git setup (do this immediately, before anyone starts working)

```
git init
```

Add a `.gitignore` (Unity's official one — search "Unity .gitignore github" or use this core list):
```
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Mm]emoryCaptures/
.vs/
.vscode/
*.csproj
*.sln
*.user
```

Enable **Git LFS** for binary assets (sprites, prefabs with large textures):
```
git lfs install
git lfs track "*.png" "*.psd" "*.wav" "*.mp3" "*.fbx" "*.anim"
```

Set **Editor → Project Settings → Editor → Version Control Mode = Visible Meta Files**, and **Asset Serialization = Force Text**. This is essential for 4 people merging scenes/prefabs without corrupting them.

---

## 2. Install Packages

**Window → Package Manager**, install:

| Package | Why |
|---|---|
| **Input System** | New input system — cleaner for mouse drag/click/hold interactions than legacy Input Manager |
| **2D Sprite** | Should come with the 2D template already |
| **TextMeshPro** | UI text (NPC dialogue, level select labels) |
| **2D Animation** (optional) | Only if furniture/NPCs need sprite-sheet animation later |
| **Newtonsoft Json** (`com.unity.nuget.newtonsoft-json`) | More robust JSON serialization than Unity's built-in `JsonUtility` — you'll need this for nested save data (rooms → furniture list → position/color) |

In **Edit → Project Settings → Player → Active Input Handling**, set to **Input System Package (New)**.

---

## 3. Folder Structure

Create this under `Assets/`:

```
Assets/
  _Project/
    Art/
      Furniture/
      Rooms/          (backgrounds per theme)
      UI/
      NPC/
    Prefabs/
      Furniture/
      NPC/
      UI/
    ScriptableObjects/
      Items/
      Rooms/
      Levels/
      NPCRequests/
    Scripts/
      Core/            (GridManager, PlacementController — Plyfah)
      Data/            (ItemDefinition, RoomDefinition, LevelDefinition — Minny)
      Progression/      (SaveSystem, LevelManager, NPCEvaluator — Spy)
      UI/              (menus, dialogue, level select — Mai)
      Utils/
    Scenes/
      MainMenu.unity
      Gameplay.unity
    Resources/          (only if you need Resources.Load — otherwise avoid)
```

This maps directly to your team split — each person mostly works inside their own `Scripts/<Area>/` folder, which minimizes merge conflicts.

---

## 4. Core Data Layer (Minny's domain) — build this first

Everything else depends on this, so build it before the placement mechanics.

### `ItemDefinition` (ScriptableObject)

```csharp
using UnityEngine;

public enum ItemCategory { Bed, Storage, Seating, Table, Lighting, Window,
    Bathroom, Kitchen, Appliance, SmallAppliance, Decor, Plant, Cushion }

public enum SurfaceBand { Floor, Wall, Ceiling, Countertop }

[CreateAssetMenu(menuName = "BannFahsai/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    public string itemId;
    public string displayName;
    public ItemCategory category;
    public Sprite icon;
    public GameObject prefab;

    [Header("Grid")]
    public Vector2Int gridSize = Vector2Int.one; // width x height in cells
    public SurfaceBand surface;

    [Header("Anchor rule (optional)")]
    public bool requiresAnchor;
    public ItemCategory anchorCategory;   // e.g. must relate to Window
    public Vector2 anchorOffsetMin;
    public Vector2 anchorOffsetMax;

    [Header("Progression")]
    public int unlockLevel = 1;
    public int maxCountAtLevel = -1;      // -1 = unlimited

    [Header("Variants")]
    public Material[] colorVariants;
    public GameObject[] meshVariants;

    [Header("Scoring")]
    public int styleScore;
    public int warmthScore;
}
```

### `RoomDefinition`

```csharp
[CreateAssetMenu(menuName = "BannFahsai/Room Definition")]
public class RoomDefinition : ScriptableObject
{
    public string roomId;
    public string displayName;
    public Sprite background;
    public Vector2Int gridDimensions;   // e.g. 12 x 8 cells
    public ItemDefinition[] fixedAnchors; // window, ceiling light — placed by design, not player
}
```

### `LevelDefinition`

```csharp
[CreateAssetMenu(menuName = "BannFahsai/Level Definition")]
public class LevelDefinition : ScriptableObject
{
    public int levelNumber;
    public string theme;
    public RoomDefinition[] unlockedRooms;
    public ItemDefinition[] newItemsThisLevel;
    public NPCRequestDefinition npcRequest;
}
```

### `NPCRequestDefinition`

```csharp
[CreateAssetMenu(menuName = "BannFahsai/NPC Request")]
public class NPCRequestDefinition : ScriptableObject
{
    public string npcName;
    public Sprite portrait;
    [TextArea] public string dialogue;

    public ItemCategory[] requiredCategories;
    public int minWarmthScore;
    public int minStyleScore;
}
```

Minny fills these in as `.asset` files via **Create → BannFahsai → ...** in the Project window — one per furniture piece, per room, per level.

---

## 5. Placement System (Plyfah's domain)

### Grid math (X/Y, side-view)

```csharp
public class GridManager : MonoBehaviour
{
    public float cellSize = 1f;
    public Vector2Int gridDimensions;
    public Vector3 gridOrigin;

    public Vector2Int WorldToCell(Vector3 worldPos)
    {
        Vector3 local = worldPos - gridOrigin;
        return new Vector2Int(
            Mathf.FloorToInt(local.x / cellSize),
            Mathf.FloorToInt(local.y / cellSize));
    }

    public Vector3 CellToWorld(Vector2Int cell)
    {
        return gridOrigin + new Vector3(cell.x * cellSize, cell.y * cellSize, 0);
    }

    public bool IsWithinSurfaceBand(Vector2Int cell, SurfaceBand band)
    {
        // e.g. Floor = bottom N rows, Ceiling = top N rows, Wall = everything between
        // implement per RoomDefinition once you fix row counts per band
        return true; // stub — fill in once room grid heights are finalized
    }
}
```

### Occupancy + collision check

Keep a `Dictionary<Vector2Int, PlacedItem>` (or a 2D bool array) of occupied cells per room. Before confirming a placement:
1. Check every cell the item's `gridSize` would occupy is inside room bounds.
2. Check none of those cells are already occupied.
3. Check `SurfaceBand` matches (floor items can't be dropped mid-wall).
4. If `requiresAnchor`, check an anchor of `anchorCategory` exists within `anchorOffsetMin/Max` of the placement point.
5. If all pass → green ghost, allow confirm. Else → red ghost, block.

This is the core "item axis related to other fixed item" logic you described — anchors are just `ItemDefinition`s tagged as anchors, and other items validate against their position at placement time, not physics.

`PlacementController` handles input (Input System — pointer down/drag/up), spawns/moves the ghost preview, calls `GridManager` for validation, and on confirm instantiates the real `PlacedItem` and registers it in the occupancy map.

---

## 6. Progression Layer (Spy's domain)

### Save data shape (JSON via Newtonsoft)

```csharp
[System.Serializable]
public class SaveData
{
    public int currentLevel;
    public List<string> unlockedRoomIds = new();
    public List<RoomSaveState> rooms = new();
}

[System.Serializable]
public class RoomSaveState
{
    public string roomId;
    public List<PlacedItemSave> items = new();
}

[System.Serializable]
public class PlacedItemSave
{
    public string itemId;
    public float x, y;
    public int rotation; // 0/90/180/270
    public int colorVariantIndex;
}
```

`SaveSystem` writes/reads this to `Application.persistentDataPath + "/save.json"`. `LevelManager` reads `currentLevel`, loads the matching `LevelDefinition`, and unlocks `unlockedRooms`. `NPCEvaluator` runs after the player confirms a room layout: sum `warmthScore`/`styleScore` across placed items, check against `NPCRequestDefinition`, pass/fail.

---

## 7. UI Layer (Mai's domain)

- **Level Select scene/panel**: reads `LevelDefinition[]` from a level list SO, shows locked/unlocked icons.
- **NPC dialogue panel**: displays `NPCRequestDefinition.portrait` + `dialogue`, shown at level start and again at evaluation (pass/fail message).
- **Color/Type picker**: on furniture select, show `ItemDefinition.colorVariants`/`meshVariants` as swatches; on tap, call into Plyfah's `PlacedItem.ApplyVariant(index)`.
- **Room switcher** (higher levels): tab/dropdown UI listing `unlockedRoomIds`, swapping the active `Gameplay` scene's loaded room.

---

## 8. Scene Setup

- **Camera**: Orthographic, sized to fit one room's grid width in the side-view.
- **Sorting Layers** (Project Settings → Tags and Layers → Sorting Layers), back to front:
  1. `RoomBackground`
  2. `WallDecor` (paintings, wall-mounted lights)
  3. `Floor`
  4. `Furniture`
  5. `GhostPreview`
  6. `UI`
- Each `ItemDefinition.prefab` should have its `SpriteRenderer.sortingLayer` set to match its `SurfaceBand`.

---

## 9. Suggested Build Order (first 2 weeks)

1. Minny: `ItemDefinition`/`RoomDefinition`/`LevelDefinition` SOs + fill Level 1 (bedroom) data only
2. Plyfah: `GridManager` + basic placement (no anchor rules yet) working with Level 1 items
3. Spy: `SaveSystem` skeleton + `LevelManager` loading Level 1
4. Mai: placeholder level-select + basic furniture picker UI
5. Integrate → get one full loop working end to end (place furniture in bedroom → save → reload) before adding anchor rules, NPC scoring, or Level 2+.

Anchor-rule validation and NPC scoring are the two riskiest systems — build them against Level 1's small item set first, don't wait until all 5 levels' data exists.

---

## 10. Open item

Level 6 is intentionally left unscaffolded per your call — when you decide, it slots into `LevelDefinition` the same way as the others; nothing above needs to change to support it later.
