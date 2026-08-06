using UnityEngine;
using UnityEngine.UI;

public class LevelCompleteUI : MonoBehaviour
{
    [Header("Check Button")]
    [SerializeField] private Button checkLevelButton;

    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Image[] starIcons; // size 5
    [SerializeField] private Sprite starFilledSprite;
    [SerializeField] private Sprite starEmptySprite;
    [SerializeField] private Text resultText;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button retryButton;

    private LevelController _controller;

    public void Init(LevelController controller)
    {
        _controller = controller;
        resultPanel.SetActive(false);
        checkLevelButton.interactable = false; // enabled once all palette items are placed

        checkLevelButton.onClick.AddListener(() => _controller.CheckLevel());

        GameEvents.OnPaletteProgressChanged += HandlePaletteProgress;
        GameEvents.OnLevelChecked += HandleLevelChecked;
    }

    private void OnDestroy()
    {
        GameEvents.OnPaletteProgressChanged -= HandlePaletteProgress;
        GameEvents.OnLevelChecked -= HandleLevelChecked;
    }

    private void HandlePaletteProgress(int placed, int total)
    {
        checkLevelButton.interactable = (placed >= total);
    }

    private void HandleLevelChecked(int stars, float overallPercent, bool canProceed)
    {
        resultPanel.SetActive(true);

        for (int i = 0; i < starIcons.Length; i++)
            starIcons[i].sprite = (i < stars) ? starFilledSprite : starEmptySprite;

        resultText.text = $"Score: {overallPercent:F0}%";

        nextLevelButton.gameObject.SetActive(canProceed);
        retryButton.gameObject.SetActive(!canProceed);
    }
}