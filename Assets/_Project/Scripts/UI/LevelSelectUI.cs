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
    private Button selectButton;
    private Button homeButton;
    private readonly List<VisualElement> levelBubbles = new();
    private int selectedLevel = 0;
    private const int LevelCount = 5;
    private const float BubbleSize = 412f;
    private const float BubbleDistance = 280f;
    private const float AnimationDuration = 0.25f;
    private bool isAnimating = false;
    [SerializeField]
    private Texture2D bubbleTexture;
    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }

    public void Setup()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }
        root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("LevelSelectUI: Root not found.");
            return;
        }

        levelArea = root.Q<VisualElement>("LevelArea");
        leftButton = root.Q<Button>("LeftButton");
        rightButton = root.Q<Button>("RightButton");
        selectButton = root.Q<Button>("SelectButton");
        homeButton = root.Q<Button>("HomeButton");

        if (levelArea == null)
        {
            Debug.LogError(
                "LevelSelectUI: LevelArea not found."
            );
            return;
        }
        if (leftButton == null)
        {
            Debug.LogError(
                "LevelSelectUI: LeftButton not found."
            );
            return;
        }
        if (rightButton == null)
        {
            Debug.LogError(
                "LevelSelectUI: RightButton not found."
            );
            return;
        }
        if (selectButton == null)
        {
            Debug.LogError(
                "LevelSelectUI: SelectButton not found."
            );
            return;
        }
        if (homeButton == null)
        {
            Debug.LogError(
                "LevelSelectUI: HomeButton not found."
            );
            return;
        }

        leftButton.clicked -= PreviousLevel;
        leftButton.clicked += PreviousLevel;
        rightButton.clicked -= NextLevel;
        rightButton.clicked += NextLevel;
        selectButton.clicked -= ConfirmLevel;
        selectButton.clicked += ConfirmLevel;
        homeButton.clicked -= GoHome;
        homeButton.clicked += GoHome;
        levelArea.pickingMode = PickingMode.Ignore;
        levelBubbles.Clear();
        selectedLevel = 0;
        isAnimating = false;
        CreateLevels();
        levelArea.RegisterCallback<GeometryChangedEvent>(
            OnLevelAreaGeometryChanged
        );
    }
    private void OnLevelAreaGeometryChanged(
        GeometryChangedEvent evt)
    {
        UpdateLevelsInstant();
    }
    private void CreateLevels()
    {
        VisualElement template =
            root.Q<VisualElement>("LevelBubble");
        if (template == null)
        {
            Debug.LogError(
                "LevelSelectUI: LevelBubble template not found."
            );
            return;
        }
        float templateWidth =
            template.resolvedStyle.width;
        float templateHeight =
            template.resolvedStyle.height;
        if (templateWidth <= 0)
        {
            templateWidth = BubbleSize;
        }
        if (templateHeight <= 0)
        {
            templateHeight = BubbleSize;
        }
        template.RemoveFromHierarchy();
        for (int i = 0; i < LevelCount; i++)
        {
            VisualElement bubble =
                CreateBubble(
                    i + 1,
                    templateWidth,
                    templateHeight
                );
            levelArea.Add(bubble);
            levelBubbles.Add(bubble);
        }
    }
    private VisualElement CreateBubble(
        int levelNumber,
        float bubbleWidth,
        float bubbleHeight)
    {
        VisualElement bubble =
            new VisualElement();
        bubble.name =
            "LevelBubble_" + levelNumber;
        bubble.pickingMode =
            PickingMode.Position;
        bubble.style.position =
            Position.Absolute;
        bubble.style.width =
            bubbleWidth;
        bubble.style.height =
            bubbleHeight;
        if (bubbleTexture != null)
        {
            bubble.style.backgroundImage =
                Background.FromTexture2D(
                    bubbleTexture
                );
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
        }
        else
        {
            Debug.LogError(
                "LevelSelectUI: Bubble Texture is NOT assigned."
            );
        }
        Label number =
            new Label(
                levelNumber.ToString()
            );
        number.name =
            "LevelNumber";
        number.pickingMode =
            PickingMode.Ignore;
        number.style.position =
            Position.Absolute;
        number.style.left = 0;
        number.style.right = 0;
        number.style.top = 0;
        number.style.bottom = 0;
        number.style.fontSize = 60;
        number.style.unityTextAlign =
            TextAnchor.MiddleCenter;
        bubble.Add(number);
        int levelIndex =
            levelNumber - 1;
        bubble.RegisterCallback<ClickEvent>(
            evt =>
            {
                SelectLevel(levelIndex);
            }
        );

        return bubble;
    }
    private void PreviousLevel()
    {
        if (isAnimating)
            return;
        if (selectedLevel <= 0)
            return;
        selectedLevel--;
        AnimateLevels();
    }
    private void NextLevel()
    {
        if (isAnimating)
            return;
        if (selectedLevel >= LevelCount - 1)
            return;
        selectedLevel++;
        AnimateLevels();
    }
    private void SelectLevel(int levelIndex)
    {
        if (isAnimating)
            return;
        if (
            levelIndex < 0 ||
            levelIndex >= LevelCount
        )
        {
            return;
        }

        if (levelIndex == selectedLevel)
            return;
        selectedLevel =
            levelIndex;
        AnimateLevels();
    }
    private void UpdateLevelsInstant()
    {
        if (levelArea == null)
            return;
        if (levelBubbles.Count == 0)
            return;
        float areaWidth =
            levelArea.resolvedStyle.width;
        float areaHeight =
            levelArea.resolvedStyle.height;
        if (
            areaWidth <= 0 ||
            areaHeight <= 0
        )
        {
            return;
        }
        float centerX =
            (areaWidth - BubbleSize) / 2f;
        for (
            int i = 0;
            i < levelBubbles.Count;
            i++
        )
        {
            VisualElement bubble =
                levelBubbles[i];
            int relativePosition =
                i - selectedLevel;
            float x =
                centerX +
                relativePosition *
                BubbleDistance;
            float scale =
                GetBubbleScale(
                    relativePosition
                );
            float y =
                (areaHeight - BubbleSize) / 2f
                + 80f;
            bubble.style.left = x;
            bubble.style.top = y;
            bubble.style.scale =
                new Scale(
                    Vector3.one * scale
                );
        }
    }
    private float GetBubbleScale(
        int relativePosition)
    {
        if (relativePosition == 0)
        {
            return 1.25f;
        }
        if (
            Mathf.Abs(relativePosition)
            == 1
        )
        {
            return 0.85f;
        }
        return 0.65f;
    }
    private void AnimateLevels()
    {
        if (levelArea == null)
            return;
        if (levelBubbles.Count == 0)
            return;
        float areaWidth =
            levelArea.resolvedStyle.width;
        float areaHeight =
            levelArea.resolvedStyle.height;
        if (
            areaWidth <= 0 ||
            areaHeight <= 0
        )
        {
            return;
        }
        float centerX =
            (areaWidth - BubbleSize) / 2f;
        float y =
            (areaHeight - BubbleSize) / 2f
            + 80f;
        isAnimating = true;
        List<float> startX =
            new List<float>();
        List<float> startScale =
            new List<float>();
        for (
            int i = 0;
            i < levelBubbles.Count;
            i++
        )
        {
            VisualElement bubble =
                levelBubbles[i];
            startX.Add(
                bubble.resolvedStyle.left
            );
            startScale.Add(
                bubble.resolvedStyle.scale.value.x
            );
        }
        float elapsed = 0f;
        levelArea.schedule
            .Execute(() =>
            {
                elapsed +=
                    Time.deltaTime;
                float t =
                    Mathf.Clamp01(
                        elapsed /
                        AnimationDuration
                    );
                float smoothT =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        t
                    );
                for (
                    int i = 0;
                    i < levelBubbles.Count;
                    i++
                )
                {
                    VisualElement bubble =
                        levelBubbles[i];
                    int relativePosition =
                        i - selectedLevel;
                    float targetX =
                        centerX +
                        relativePosition *
                        BubbleDistance;
                    float targetScale =
                        GetBubbleScale(
                            relativePosition
                        );
                    float currentX =
                        Mathf.Lerp(
                            startX[i],
                            targetX,
                            smoothT
                        );
                    float currentScale =
                        Mathf.Lerp(
                            startScale[i],
                            targetScale,
                            smoothT
                        );
                    bubble.style.left =
                        currentX;
                    bubble.style.top =
                        y;

                    bubble.style.scale =
                        new Scale(
                            Vector3.one *
                            currentScale
                        );
                }
                if (t >= 1f)
                {
                    isAnimating = false;
                }
            })
            .Every(16);
    }
    private void ConfirmLevel()
    {
        int level =
            selectedLevel + 1;
        Debug.Log(
            "Selected Level: " + level
        );
    }
    private void GoHome()
    {
        Debug.Log(
            "Home button pressed."
        );
    }
}