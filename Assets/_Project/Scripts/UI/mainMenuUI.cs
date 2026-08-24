using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset mainMenu;
    [SerializeField] private VisualTreeAsset levelSelect;

    private UIDocument uiDocument;

    private void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();

        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        uiDocument.visualTreeAsset = mainMenu;

        VisualElement root = uiDocument.rootVisualElement;

        Button playButton = root.Q<Button>("play-button");

        if (playButton != null)
        {
            playButton.clicked -= ShowLevelSelect;
            playButton.clicked += ShowLevelSelect;
        }
    }

    private void ShowLevelSelect()
    {
        uiDocument.visualTreeAsset = levelSelect;

        VisualElement root = uiDocument.rootVisualElement;

        Button HomeButton = root.Q<Button>("HomeButton");

        if (HomeButton != null)
        {
            HomeButton.clicked -= ShowMainMenu;
            HomeButton.clicked += ShowMainMenu;
        }

        LevelSelectUI levelSelectUI = GetComponent<LevelSelectUI>();

        if (levelSelectUI != null)
        {
            levelSelectUI.Setup();
        }
    }
}