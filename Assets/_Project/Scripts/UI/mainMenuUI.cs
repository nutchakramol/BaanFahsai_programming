using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset mainMenu;
    [SerializeField] private VisualTreeAsset levelSelect;

    private UIDocument uiDocument;

    private void Awake()
    {
        uiDocument =
            GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        uiDocument.visualTreeAsset =
            mainMenu;

        VisualElement root =
            uiDocument.rootVisualElement;

        Button playButton =
            root.Q<Button>("play-button");

        if (playButton != null)
        {
            playButton.clicked -=
                ShowLevelSelect;

            playButton.clicked +=
                ShowLevelSelect;
        }
    }

    private void ShowLevelSelect()
    {
        uiDocument.visualTreeAsset =
            levelSelect;

        VisualElement root =
            uiDocument.rootVisualElement;

        Button homeButton =
            root.Q<Button>("HomeButton");

        if (homeButton != null)
        {
            homeButton.clicked -=
                ShowMainMenu;

            homeButton.clicked +=
                ShowMainMenu;
        }

        LevelSelectUI levelSelectUI =
            GetComponent<LevelSelectUI>();

        if (levelSelectUI != null)
        {
            levelSelectUI.Setup();
        }
    }
}