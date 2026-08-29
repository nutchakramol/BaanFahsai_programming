using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FurnitureHotbarUI : MonoBehaviour
{
    [System.Serializable]
    public class FurnitureSlot
    {
        public Button button;
        public Image icon;
        public GameObject lockIcon;
        public GameObject selectionHighlight;
    }

    [Header("Visible Slots")]
    public FurnitureSlot[] slots;

    [Header("Page Buttons")]
    public Button previousButton;
    public Button nextButton;

    [Header("Optional Page Text")]
    public TMP_Text pageText;

    private GameObject[] currentFurniture;

    private int currentPage = 0;
    private int selectedFurnitureIndex = -1;

    private int ItemsPerPage
    {
        get
        {
            return slots != null
                ? slots.Length
                : 0;
        }
    }

    private int PageCount
    {
        get
        {
            if (currentFurniture == null ||
                currentFurniture.Length == 0 ||
                ItemsPerPage == 0)
            {
                return 1;
            }

            return Mathf.CeilToInt(
                (float)currentFurniture.Length /
                ItemsPerPage
            );
        }
    }


    public void SetFurnitureList(
        GameObject[] furnitureList)
    {
        currentFurniture = furnitureList;

        currentPage = 0;
        selectedFurnitureIndex =
            FindFirstUnlockedIndex();

        if (selectedFurnitureIndex >= 0 &&
            PlacementController.Instance != null)
        {
            PlacementController.Instance
                .SelectFurniture(
                    selectedFurnitureIndex
                );
        }

        RefreshSlots();

        Debug.Log(
            $"[FurnitureHotbarUI] Furniture count: " +
            $"{(currentFurniture != null ? currentFurniture.Length : 0)}, " +
            $"Pages: {PageCount}"
        );
    }

    private void RefreshSlots()
    {
        if (slots == null ||
            ItemsPerPage == 0)
        {
            return;
        }

        int startIndex =
            currentPage * ItemsPerPage;

        for (int slotIndex = 0;
             slotIndex < slots.Length;
             slotIndex++)
        {
            FurnitureSlot slot =
                slots[slotIndex];

            if (slot == null ||
                slot.button == null)
            {
                continue;
            }

            int furnitureIndex =
                startIndex + slotIndex;

            if (currentFurniture == null ||
                furnitureIndex >=
                currentFurniture.Length ||
                currentFurniture[furnitureIndex] == null)
            {
                slot.button.gameObject.SetActive(false);
                continue;
            }

            slot.button.gameObject.SetActive(true);

            GameObject prefab =
                currentFurniture[furnitureIndex];

            FurnitureItem item =
                prefab.GetComponent<FurnitureItem>();

            bool unlocked =
                item == null ||
                item.IsUnlockedForPlayer();

            // -------------------------
            // Sprite
            // -------------------------

            SpriteRenderer sr =
                prefab.GetComponent<SpriteRenderer>();

            if (sr == null)
            {
                sr =
                    prefab.GetComponentInChildren<SpriteRenderer>();
            }

            if (slot.icon != null)
            {
                slot.icon.sprite =
                    sr != null
                        ? sr.sprite
                        : null;

                slot.icon.enabled =
                    slot.icon.sprite != null;

                slot.icon.preserveAspect = true;
            }

            // -------------------------
            // Lock
            // -------------------------

            if (slot.lockIcon != null)
            {
                slot.lockIcon.SetActive(!unlocked);
            }

            slot.button.interactable = unlocked;

            // -------------------------
            // Click
            // -------------------------

            slot.button.onClick.RemoveAllListeners();

            int capturedIndex =
                furnitureIndex;

            if (unlocked)
            {
                slot.button.onClick.AddListener(
                    () =>
                        SelectFurniture(
                            capturedIndex
                        )
                );
            }

            // -------------------------
            // Selected border
            // -------------------------

            if (slot.selectionHighlight != null)
            {
                slot.selectionHighlight.SetActive(
                    unlocked &&
                    furnitureIndex ==
                    selectedFurnitureIndex
                );
            }
        }

        UpdatePageButtons();
    }

    private void SelectFurniture(int index)
    {
        if (currentFurniture == null ||
            index < 0 ||
            index >= currentFurniture.Length)
        {
            return;
        }

        selectedFurnitureIndex = index;

        if (PlacementController.Instance != null)
        {
            PlacementController.Instance
                .SelectFurniture(index);
        }

        RefreshSlots();

        Debug.Log(
            $"[FurnitureHotbarUI] Selected: " +
            $"{currentFurniture[index].name}"
        );
    }

    public void NextPage()
{
    Debug.Log(
        $"[FurnitureHotbarUI] NEXT clicked. " +
        $"Current page: {currentPage}, Page count: {PageCount}"
    );

    if (currentPage >= PageCount - 1)
    {
        Debug.Log(
            "[FurnitureHotbarUI] Already on last page."
        );
        return;
    }

    currentPage++;

    Debug.Log(
        $"[FurnitureHotbarUI] Changed to page {currentPage + 1}"
    );

    RefreshSlots();
}

public void PreviousPage()
{
    Debug.Log(
        $"[FurnitureHotbarUI] PREVIOUS clicked. " +
        $"Current page: {currentPage}"
    );

    if (currentPage <= 0)
    {
        Debug.Log(
            "[FurnitureHotbarUI] Already on first page."
        );
        return;
    }

    currentPage--;

    Debug.Log(
        $"[FurnitureHotbarUI] Changed to page {currentPage + 1}"
    );

    RefreshSlots();
}
    private void UpdatePageButtons()
    {
        if (previousButton != null)
        {
            previousButton.interactable =
                currentPage > 0;
        }

        if (nextButton != null)
        {
            nextButton.interactable =
                currentPage <
                PageCount - 1;
        }

        if (pageText != null)
        {
            pageText.text =
                $"{currentPage + 1}/{PageCount}";
        }
    }

    private int FindFirstUnlockedIndex()
    {
        if (currentFurniture == null)
            return -1;

        for (int i = 0;
             i < currentFurniture.Length;
             i++)
        {
            if (currentFurniture[i] == null)
                continue;

            FurnitureItem item =
                currentFurniture[i]
                    .GetComponent<FurnitureItem>();

            if (item == null ||
                item.IsUnlockedForPlayer())
            {
                return i;
            }
        }

        return -1;
    }
}