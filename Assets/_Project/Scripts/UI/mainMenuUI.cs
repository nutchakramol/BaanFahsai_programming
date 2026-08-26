using UnityEngine;
using UnityEngine.UIElements;
 
public class MainMenuUI : MonoBehaviour
{
    [SerializeField]
    private VisualTreeAsset mainMenu;
 
    [SerializeField]
    private VisualTreeAsset levelSelect;
 
 
    private UIDocument uiDocument;
<<<<<<< HEAD
 
 
=======

>>>>>>> 966d99285b3dbaf33b3f239b35e13e828f18fec7
    private void Awake()
    {
        uiDocument =
            GetComponent<UIDocument>();
    }
<<<<<<< HEAD
 
 
    private void OnEnable()
    {
        if (uiDocument == null)
        {
            uiDocument =
                GetComponent<UIDocument>();
        }
 
 
        ShowMainMenu();
    }
 
 
    // =========================================================
    // MAIN MENU
    // =========================================================
 
    private void ShowMainMenu()
    {
        uiDocument.visualTreeAsset =
            mainMenu;
 
 
        VisualElement root =
            uiDocument.rootVisualElement;
 
 
        if (root == null)
            return;
 
 
        Button playButton =
            root.Q<Button>("play-button");
 
 
=======

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

>>>>>>> 966d99285b3dbaf33b3f239b35e13e828f18fec7
        if (playButton != null)
        {
            playButton.clicked -=
                ShowLevelSelect;
<<<<<<< HEAD
 
 
            playButton.clicked +=
                ShowLevelSelect;
        }
        else
        {
            Debug.LogError(
                "MainMenuUI: play-button not found."
            );
=======

            playButton.clicked +=
                ShowLevelSelect;
>>>>>>> 966d99285b3dbaf33b3f239b35e13e828f18fec7
        }
    }
 
 
    // =========================================================
    // LEVEL SELECT
    // =========================================================
 
    private void ShowLevelSelect()
    {
        uiDocument.visualTreeAsset =
            levelSelect;
<<<<<<< HEAD
 
 
        VisualElement root =
            uiDocument.rootVisualElement;
 
 
        if (root == null)
            return;
 
 
        // =====================================================
        // HOME
        // =====================================================
 
        Button homeButton =
            root.Q<Button>("HomeButton");
 
 
=======

        VisualElement root =
            uiDocument.rootVisualElement;

        Button homeButton =
            root.Q<Button>("HomeButton");

>>>>>>> 966d99285b3dbaf33b3f239b35e13e828f18fec7
        if (homeButton != null)
        {
            homeButton.clicked -=
                ShowMainMenu;
<<<<<<< HEAD
 
 
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
 
 
=======

            homeButton.clicked +=
                ShowMainMenu;
        }

        LevelSelectUI levelSelectUI =
            GetComponent<LevelSelectUI>();

>>>>>>> 966d99285b3dbaf33b3f239b35e13e828f18fec7
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