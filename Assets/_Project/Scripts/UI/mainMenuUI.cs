using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField]
    private VisualTreeAsset mainMenu;

    [SerializeField]
    private VisualTreeAsset levelSelect;

    [SerializeField] private VisualTreeAsset settingsScreen;


    private UIDocument uiDocument;


    private void Awake()
    {
        uiDocument =
            GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        if (uiDocument == null)
        {
            uiDocument =
                GetComponent<UIDocument>();
        }

        ShowMainMenu();
    }
    public void ShowSettings()
   {
       uiDocument.visualTreeAsset = settingsScreen;
       VisualElement root = uiDocument.rootVisualElement;
       Button backButton = root.Q<Button>("BackButton");
       if (backButton != null)
       {
           backButton.clicked -= ShowMainMenu;
           backButton.clicked += ShowMainMenu;
       }
   }

    // =========================================================
    // MAIN MENU
    // =========================================================

    public void ShowMainMenu()
    {
        uiDocument.visualTreeAsset =
            mainMenu;


        VisualElement root =
            uiDocument.rootVisualElement;


        if (root == null)
            return;


        Button playButton =
            root.Q<Button>("play-button");


        if (playButton != null)
        {
            playButton.clicked -=
                ShowLevelSelect;

            playButton.clicked +=
                ShowLevelSelect;
        }
        else
        {
            Debug.LogError(
                "MainMenuUI: play-button not found."
            );
        }
    }

    // =========================================================
    // LEVEL SELECT
    // =========================================================

    private void ShowLevelSelect()
    {
        uiDocument.visualTreeAsset =
            levelSelect;


        VisualElement root =
            uiDocument.rootVisualElement;


        if (root == null)
            return;


        // =====================================================
        // HOME
        // =====================================================

        Button homeButton =
            root.Q<Button>("HomeButton");

        if (homeButton != null)
        {
            homeButton.clicked -=
                ShowMainMenu;

            homeButton.clicked +=
                ShowMainMenu;
        }
        else
        {
            Debug.LogError(
                "MainMenuUI: HomeButton not found."
            );
        }

        // =====================================================
        // LEVEL SELECT SCRIPT
        // =====================================================

        LevelSelectUI levelSelectUI =
            GetComponent<LevelSelectUI>();

        if (levelSelectUI != null)
        {
            levelSelectUI.Setup();
        }
        else
        {
            Debug.LogError(
                "MainMenuUI: LevelSelectUI component not found."
            );
        }
        
    }
}