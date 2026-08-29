using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class LevelSelectUI : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement levelArea;
    private Button leftButton;
    private Button rightButton;
    private Button selectButton;
    private Button homeButton;
    private readonly List<VisualElement> levelBubbles = new();

    [SerializeField] private List<LevelDataSO> levels;
    [Header("To Be Continued Popup")]
    [SerializeField] private Sprite toBeContinuedSprite;
    private VisualElement toBeContinuedPopup;
    private int selectedLevel = 0;

    private int LevelCount => levels != null ? levels.Count : 0;

    private const float BubbleWidth = 412f;
    private const float BubbleHeight = 412f;
    private const float BubbleDistance = 470f;
    private const float CenterScale = 1.1f;
    private const float SideScale = 0.75f;
    private const float FarScale = 0.55f;
    private const float AnimationDuration = 0.35f;

    private bool isAnimating;
    private float animationStartTime;

    private readonly List<float> startX = new();
    private readonly List<float> startY = new();
    private readonly List<float> startScale = new();
    private readonly List<float> targetX = new();
    private readonly List<float> targetY = new();
    private readonly List<float> targetScale = new();

    private IVisualElementScheduledItem animationSchedule;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }
    public void Setup()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        root = uiDocument.rootVisualElement;

        if (root == null)
        {
            Debug.LogError("LevelSelectUI: Root not found.");
            return;
        }

        if (levels == null || levels.Count == 0)
        {
            Debug.LogError(
                "LevelSelectUI: No LevelDataSO assets assigned in the Inspector."
            );
            return;
        }

        levelArea = root.Q<VisualElement>("LevelArea");
        leftButton = root.Q<Button>("LeftButton");
        rightButton = root.Q<Button>("RightButton");
        selectButton = root.Q<Button>("SelectButton");
        homeButton = root.Q<Button>("HomeButton");

        if (levelArea == null)
        {
            Debug.LogError("LevelSelectUI: LevelArea not found.");
            return;
        }

        if (leftButton == null)
        {
            Debug.LogError("LevelSelectUI: LeftButton not found.");
            return;
        }

        if (rightButton == null)
        {
            Debug.LogError("LevelSelectUI: RightButton not found.");
            return;
        }

        if (selectButton == null)
        {
            Debug.LogError("LevelSelectUI: SelectButton not found.");
            return;
        }

        if (homeButton == null)
        {
            Debug.LogError("LevelSelectUI: HomeButton not found.");
            return;
        }

        levelArea.pickingMode = PickingMode.Ignore;

        leftButton.clicked -= PreviousLevel;
        rightButton.clicked -= NextLevel;
        selectButton.clicked -= ConfirmLevel;
        homeButton.clicked -= GoHome;

        leftButton.clicked += PreviousLevel;
        rightButton.clicked += NextLevel;
        selectButton.clicked += ConfirmLevel;
        homeButton.clicked += GoHome;

        levelBubbles.Clear();
        CreateLevels();

        CreateToBeContinuedPopup();

        selectedLevel = 0;
        isAnimating = false;

        levelArea.RegisterCallback<GeometryChangedEvent>(
            OnLevelAreaGeometryChanged
        );

        levelArea.schedule
            .Execute(() => UpdateLevels(true))
            .StartingIn(50);
    }

    private void OnLevelAreaGeometryChanged(GeometryChangedEvent evt)
    {
        if (!isAnimating)
            UpdateLevels(true);
    }

    private void CreateLevels()
    {
        levelBubbles.Clear();

        VisualElement template = root.Q<VisualElement>("LevelBubble");

        if (template == null)
        {
            Debug.LogError(
                "LevelSelectUI: LevelBubble template not found."
            );
            return;
        }

        Background bubbleBackground =
            template.resolvedStyle.backgroundImage;

        for (int i = 0; i < LevelCount; i++)
        {
            VisualElement bubble = new VisualElement();

            bubble.name = "LevelBubble_" + (i + 1);
            bubble.pickingMode = PickingMode.Position;
            bubble.style.position = Position.Absolute;
            bubble.style.width = BubbleWidth;
            bubble.style.height = BubbleHeight;
            bubble.style.backgroundImage = bubbleBackground;

            bubble.style.backgroundRepeat =
                new BackgroundRepeat(
                    Repeat.NoRepeat,
                    Repeat.NoRepeat
                );

            bubble.style.backgroundSize =
                new BackgroundSize(
                    Length.Percent(100),
                    Length.Percent(100)
                );

            Label number = new Label((i + 1).ToString());

            number.name = "LevelNumber";
            number.pickingMode = PickingMode.Ignore;
            number.style.position = Position.Absolute;
            number.style.left = 0;
            number.style.right = 0;
            number.style.top = 0;
            number.style.bottom = 0;
            number.style.fontSize = 60;
            number.style.unityTextAlign =
                TextAnchor.MiddleCenter;

            bubble.Add(number);

            VisualElement crown =
                CreateIcon("Crown", template);

            VisualElement star1 =
                CreateIcon("Star1", template);

            VisualElement star2 =
                CreateIcon("Star2", template);

            VisualElement star3 =
                CreateIcon("Star3", template);

            VisualElement lockIcon =
                CreateIcon("Lock", template);

            // ===== CHANGED BLOCK START =====
            bool unlocked = LevelProgress.IsUnlocked(i);
            int starsEarned = LevelProgress.GetStars(i);

            crown.style.display =
                unlocked
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

            star1.style.display =
                (unlocked && starsEarned >= 1)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

            star2.style.display =
                (unlocked && starsEarned >= 2)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

            star3.style.display =
                (unlocked && starsEarned >= 3)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

            lockIcon.style.display =
                unlocked
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
            // ===== CHANGED BLOCK END =====

            bubble.Add(crown);
            bubble.Add(star1);
            bubble.Add(star2);
            bubble.Add(star3);
            bubble.Add(lockIcon);

            int levelIndex = i;

            bubble.RegisterCallback<ClickEvent>(
                evt =>
                {
                    SelectLevel(levelIndex);
                    evt.StopPropagation();
                }
            );

            levelArea.Add(bubble);
            levelBubbles.Add(bubble);
        }

        template.RemoveFromHierarchy();
    }

    private VisualElement CreateIcon(
        string imageName,
        VisualElement template
    )
    {
        VisualElement original =
            template.Q<VisualElement>(imageName);

        VisualElement icon =
            new VisualElement();

        icon.name = imageName;
        icon.pickingMode = PickingMode.Ignore;
        icon.style.position = Position.Absolute;

        if (original != null)
        {
            icon.style.backgroundImage =
                original.resolvedStyle.backgroundImage;

            icon.style.backgroundSize =
                original.resolvedStyle.backgroundSize;

            icon.style.backgroundRepeat =
                original.resolvedStyle.backgroundRepeat;
        }

        if (imageName == "Crown")
        {
            icon.style.width = 100;
            icon.style.height = 100;
            icon.style.left = 156;
            icon.style.top = -55;
        }
        else if (imageName == "Star1")
        {
            icon.style.width = 50;
            icon.style.height = 50;
            icon.style.left = 135;
            icon.style.top = 370;
        }
        else if (imageName == "Star2")
        {
            icon.style.width = 50;
            icon.style.height = 50;
            icon.style.left = 181;
            icon.style.top = 370;
        }
        else if (imageName == "Star3")
        {
            icon.style.width = 50;
            icon.style.height = 50;
            icon.style.left = 227;
            icon.style.top = 370;
        }
        else if (imageName == "Lock")
        {
            icon.style.width = 100;
            icon.style.height = 100;
            icon.style.left = 156;
            icon.style.top = 156;
            icon.style.display = DisplayStyle.None;
        }

        return icon;
    }

    private void PreviousLevel()
    {
        if (isAnimating)
            return;

        if (selectedLevel <= 0)
            return;

        selectedLevel--;
        StartAnimation();
    }

    private void NextLevel()
    {
        if (isAnimating)
            return;

        if (selectedLevel >= LevelCount - 1)
        {
            selectedLevel = LevelCount;
            StartAnimation();
            levelArea.schedule
                .Execute(() => ShowToBeContinuedPopup())
                .StartingIn((long)(AnimationDuration * 1000));
            return;
        }

        selectedLevel++;
        StartAnimation();
    }

    private void SelectLevel(int levelIndex)
    {
        if (isAnimating)
            return;

        if (levelIndex < 0 ||
            levelIndex >= LevelCount)
            return;

        if (levelIndex == selectedLevel)
            return;

        selectedLevel = levelIndex;
        StartAnimation();
    }

    private void UpdateLevels(bool instant)
    {
        if (levelArea == null)
            return;

        if (levelBubbles.Count == 0)
            return;

        float areaWidth =
            levelArea.resolvedStyle.width;

        float areaHeight =
            levelArea.resolvedStyle.height;

        if (areaWidth <= 0 ||
            areaHeight <= 0)
            return;

        float centerX =
            (areaWidth - BubbleWidth) / 2f;

        float centerY =
            (areaHeight - BubbleHeight) / 2f;

        for (int i = 0;
             i < levelBubbles.Count;
             i++)
        {
            VisualElement bubble =
                levelBubbles[i];

            int relativePosition =
                i - selectedLevel;

            float x =
                centerX +
                relativePosition *
                BubbleDistance;

            float scale;

            if (relativePosition == 0)
                scale = CenterScale;
            else if (Mathf.Abs(relativePosition) == 1)
                scale = SideScale;
            else
                scale = FarScale;

            bubble.style.left = x;
            bubble.style.top = centerY;
            bubble.style.scale =
                new Scale(Vector3.one * scale);
        }

        isAnimating = false;
    }

    private void StartAnimation()
    {
        if (levelArea == null)
            return;

        if (levelBubbles.Count == 0)
            return;

        if (animationSchedule != null)
            animationSchedule.Pause();

        float areaWidth =
            levelArea.resolvedStyle.width;

        float areaHeight =
            levelArea.resolvedStyle.height;

        if (areaWidth <= 0 ||
            areaHeight <= 0)
        {
            UpdateLevels(true);
            return;
        }

        float centerX =
            (areaWidth - BubbleWidth) / 2f;

        float centerY =
            (areaHeight - BubbleHeight) / 2f;

        startX.Clear();
        startY.Clear();
        startScale.Clear();

        targetX.Clear();
        targetY.Clear();
        targetScale.Clear();

        for (int i = 0;
             i < levelBubbles.Count;
             i++)
        {
            VisualElement bubble =
                levelBubbles[i];

            int relativePosition =
                i - selectedLevel;

            float targetPositionX =
                centerX +
                relativePosition *
                BubbleDistance;

            float targetPositionScale;

            if (relativePosition == 0)
                targetPositionScale = CenterScale;
            else if (Mathf.Abs(relativePosition) == 1)
                targetPositionScale = SideScale;
            else
                targetPositionScale = FarScale;

            float currentX =
                bubble.resolvedStyle.left;

            float currentY =
                bubble.resolvedStyle.top;

            Vector3 currentScale =
                bubble.resolvedStyle.scale.value;

            startX.Add(currentX);
            startY.Add(currentY);
            startScale.Add(currentScale.x);

            targetX.Add(targetPositionX);
            targetY.Add(centerY);
            targetScale.Add(targetPositionScale);
        }

        animationStartTime =
            Time.realtimeSinceStartup;

        isAnimating = true;

        animationSchedule =
            levelArea.schedule.Execute(
                AnimateLevels
            );

        animationSchedule.Every(16);
    }

    private void AnimateLevels(TimerState timer)
    {
        if (!isAnimating)
            return;

        float elapsed =
            Time.realtimeSinceStartup -
            animationStartTime;

        float t =
            Mathf.Clamp01(
                elapsed /
                AnimationDuration
            );

        t = Mathf.SmoothStep(
            0f,
            1f,
            t
        );

        for (int i = 0;
             i < levelBubbles.Count;
             i++)
        {
            VisualElement bubble =
                levelBubbles[i];

            float x =
                Mathf.Lerp(
                    startX[i],
                    targetX[i],
                    t
                );

            float y =
                Mathf.Lerp(
                    startY[i],
                    targetY[i],
                    t
                );

            float scale =
                Mathf.Lerp(
                    startScale[i],
                    targetScale[i],
                    t
                );

            bubble.style.left = x;
            bubble.style.top = y;

            bubble.style.scale =
                new Scale(
                    Vector3.one * scale
                );
        }

        if (t >= 1f)
        {
            isAnimating = false;

            if (animationSchedule != null)
                animationSchedule.Pause();
        }
    }

// =========================================================
// ADD: TO BE CONTINUED POPUP
// =========================================================

    private void CreateToBeContinuedPopup()
    {
        if (toBeContinuedPopup != null)
            toBeContinuedPopup.RemoveFromHierarchy();

        toBeContinuedPopup = new VisualElement();

        toBeContinuedPopup.name = "ToBeContinuedPopup";

        toBeContinuedPopup.style.position = Position.Absolute;
        toBeContinuedPopup.style.left = 0;
        toBeContinuedPopup.style.right = 0;
        toBeContinuedPopup.style.top = 0;
        toBeContinuedPopup.style.bottom = 0;

        toBeContinuedPopup.style.justifyContent =
            Justify.Center;

        toBeContinuedPopup.style.alignItems =
            Align.Center;

        toBeContinuedPopup.style.backgroundColor =
            new Color(0f, 0f, 0f, 0.35f);

        VisualElement messageBox =
            new VisualElement();

        messageBox.name = "MessageBox";

        messageBox.style.width = 600;
        messageBox.style.height = 400;

        if (toBeContinuedSprite != null)
        {
            messageBox.style.backgroundImage =
                Background.FromSprite(
                    toBeContinuedSprite
                );
        }

        messageBox.style.backgroundRepeat =
            new BackgroundRepeat(
                Repeat.NoRepeat,
                Repeat.NoRepeat
            );

        messageBox.style.backgroundSize =
            new BackgroundSize(
                Length.Percent(100),
                Length.Percent(100)
            );

        messageBox.style.justifyContent =
            Justify.Center;

        messageBox.style.alignItems =
            Align.Center;

        Label message =
            new Label("To be continue...");

        message.name =
            "ToBeContinuedText";

        message.style.color =
            Color.black;

        // ตัวหนา
        message.style.unityFontStyleAndWeight =
            FontStyle.Bold;

        message.style.fontSize = 42;

        message.style.unityTextAlign =
            TextAnchor.MiddleCenter;

        message.style.marginBottom = 35;

        messageBox.Add(message);

        Button closeButton =
            new Button(HideToBeContinuedPopup);

        closeButton.name =
            "ToBeContinuedCloseButton";

        closeButton.text = "OK";

        closeButton.style.width = 150;
        closeButton.style.height = 60;

        closeButton.style.fontSize = 26;

        closeButton.style.unityFontStyleAndWeight =
            FontStyle.Bold;

        closeButton.style.color =
            Color.black;

        messageBox.Add(closeButton);

        toBeContinuedPopup.Add(messageBox);

        toBeContinuedPopup.style.display =
            DisplayStyle.None;

        root.Add(toBeContinuedPopup);
    }

    // ////////////////////

    private void ShowToBeContinuedPopup()
    {
        if (toBeContinuedPopup == null)
            CreateToBeContinuedPopup();

        toBeContinuedPopup.style.display =
            DisplayStyle.Flex;
    }

    // ////////////////////

    private void HideToBeContinuedPopup()
    {
        if (toBeContinuedPopup == null)
            return;

        toBeContinuedPopup.style.display =
            DisplayStyle.None;
    }

    // ////////////////////
    private void ConfirmLevel()
    {
        if (!LevelProgress.IsUnlocked(selectedLevel))
        {
            Debug.Log("This level is locked.");
            return;
        }

        if (selectedLevel < 0 ||
            selectedLevel >= LevelCount)
        {
            Debug.LogError(
                "LevelSelectUI: Invalid selected level."
            );
            return;
        }

        LevelDataSO data =
            levels[selectedLevel];

        if (data == null)
        {
            Debug.LogError(
                "LevelSelectUI: LevelData is null for Level " +
                (selectedLevel + 1)
            );
            return;
        }

        if (string.IsNullOrEmpty(data.sceneName))
        {
            Debug.LogError(
                "LevelSelectUI: Scene name is empty for Level " +
                (selectedLevel + 1)
            );
            return;
        }

        Debug.Log(
            "Loading Level: " +
            data.levelIndex +
            " → " +
            data.sceneName
        );

        GameEvents.RaiseLevelSelected(data);

        SceneManager.LoadScene(data.sceneName);
    }

    private void GoHome()
    {
        MainMenuUI mainMenu =
            GetComponent<MainMenuUI>();

        if (mainMenu != null)
        {
            mainMenu.ShowMainMenu();
        }
        else
        {
            Debug.LogError(
                "LevelSelectUI: MainMenuUI component not found."
            );
        }
    }
}