using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro namespace

public class ScoreHUD : MonoBehaviour
{
    [SerializeField] private Slider scoreBar;
    [SerializeField] private TextMeshProUGUI scoreLabel; // changed from Text

    private void OnEnable()
    {
        GameEvents.OnLevelScoreUpdated += HandleScoreUpdated;
        GameEvents.OnLevelCompleted += HandleLevelCompleted;
    }

    private void OnDisable()
    {
        GameEvents.OnLevelScoreUpdated -= HandleScoreUpdated;
        GameEvents.OnLevelCompleted -= HandleLevelCompleted;
    }

    private void HandleScoreUpdated(LevelScoreResult result)
    {
        if (scoreBar != null) scoreBar.value = result.OverallScorePercent / 100f;
        if (scoreLabel != null) scoreLabel.text = $"{result.OverallScorePercent:F0}%";
    }

    private void HandleLevelCompleted()
    {
        Debug.Log("Level Complete!");
    }
}