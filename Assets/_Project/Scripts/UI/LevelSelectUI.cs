using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelSelectUI : MonoBehaviour
{
    private UIDocument uiDocument;

    private VisualElement root;
    private VisualElement levelArea;
    private Button leftButton;
    private Button rightButton;
    private readonly List<VisualElement> levelBubbles = new();
    private int selectedLevel = 0;
    private const int LevelCount = 5;
    private const float BubbleSize =400f;
    private const float BubbleDistance = 280f;
    private bool isSetup = false;
    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }

    public void Setup()
    {
        if (isSetup)
            return;
        root = uiDocument.rootVisualElement;
        levelArea = root.Q<VisualElement>("LevelArea");
        leftButton = root.Q<Button>("LeftButton");
        rightButton = root.Q<Button>("RightButton");
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
        leftButton.clicked += PreviousLevel;
        rightButton.clicked += NextLevel;
        CreateLevels();
        isSetup = true;
        levelArea.RegisterCallback<GeometryChangedEvent>(_ =>
        {
            UpdateLevels();
        });
    }

    private void CreateLevels()
    {
        VisualElement template = root.Q<VisualElement>("LevelBubble");
        if (template == null)
        {
            Debug.LogError("LevelSelectUI: LevelBubble not found.");
            return;
        }

        Background bubbleBackground = template.resolvedStyle.backgroundImage;
        float templateWidth = template.resolvedStyle.width;
        float templateHeight = template.resolvedStyle.height;
        template.RemoveFromHierarchy();
        for (int i = 0; i < LevelCount; i++)
        {
            VisualElement bubble = CreateBubble(
                i + 1,
                bubbleBackground,
                templateWidth,
                templateHeight
            );
            levelArea.Add(bubble);
            levelBubbles.Add(bubble);
        }
    }

    private VisualElement CreateBubble(
        int levelNumber,
        Background bubbleBackground,
        float bubbleWidth,
        float bubbleHeight)
    {
        VisualElement bubble = new VisualElement();
        bubble.name = "LevelBubble_" + levelNumber;
        bubble.style.position = Position.Absolute;
        bubble.style.width = bubbleWidth;
        bubble.style.height = bubbleHeight;
        bubble.style.backgroundImage = bubbleBackground;
        bubble.style.backgroundRepeat = new BackgroundRepeat(
            Repeat.NoRepeat,
            Repeat.NoRepeat
        );
        bubble.style.backgroundSize = new BackgroundSize(
            Length.Percent(100),
            Length.Percent(100)
        );
        Label number = new Label(levelNumber.ToString());
        number.name = "LevelNumber";
        number.style.position = Position.Absolute;
        number.style.left = 0;
        number.style.right = 0;
        number.style.top = 0;
        number.style.bottom = 0;
        number.style.fontSize = 60;
        number.style.unityTextAlign = TextAnchor.MiddleCenter;
        bubble.Add(number);
        int levelIndex = levelNumber - 1;
        bubble.RegisterCallback<ClickEvent>(_ =>
        {
            SelectLevel(levelIndex);
        });
        return bubble;
    }

    private void PreviousLevel()
    {
        if (selectedLevel <= 0)
            return;
        selectedLevel--;
        UpdateLevels();
    }

    private void NextLevel()
    {
        if (selectedLevel >= LevelCount - 1)
            return;
        selectedLevel++;
        UpdateLevels();
    }

    private void SelectLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= LevelCount)
            return;
        selectedLevel = levelIndex;
        UpdateLevels();
    }

    private void UpdateLevels()
    {
        if (levelArea == null)
            return;
        float areaWidth = levelArea.resolvedStyle.width;
        if (areaWidth <= 0)
            return;
        float centerX =
            (areaWidth - BubbleSize) / 2f;
        for (int i = 0; i < levelBubbles.Count; i++)
        {
            int relativePosition = i - selectedLevel;
            float x =
                centerX +
                relativePosition * BubbleDistance;
            float scale = 0.7f;
            if (relativePosition == 0)
            {
                scale = 1.25f;
            }
            else if (Mathf.Abs(relativePosition) == 1)
            {
                scale = 0.9f;
            }
            float areaHeight = levelArea.resolvedStyle.height;
            float y = (areaHeight - BubbleSize) / 2f + 80f;
            levelBubbles[i].style.left = x;
            levelBubbles[i].style.top = y;
            levelBubbles[i].style.scale =
                new Scale(Vector3.one * scale);
        }
    }
}