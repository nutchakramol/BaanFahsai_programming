using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class NPCDialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Image npcPortraitImage;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button continueButton;

    private List<string> _currentDialogue;
    private int _currentLineIndex;
    private Action _onDialogueFinished;

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(HandleContinue);
    }

    /// <summary>
    /// Shows the NPC introduction dialogue one line at a time.
    /// </summary>
    public void Show(LevelDataSO levelData, Action onContinue)
    {
        if (levelData == null)
        {
            Debug.LogError("NPCDialogueUI: LevelData is null.");
            return;
        }

        _onDialogueFinished = onContinue;

        // NPC information
        if (npcNameText != null)
            npcNameText.text = levelData.npcName;

        if (npcPortraitImage != null)
            npcPortraitImage.sprite = levelData.npcPortrait;

        // Copy dialogue from LevelDataSO
        _currentDialogue = levelData.npcIntroDialogue;

        _currentLineIndex = 0;

        if (_currentDialogue == null || _currentDialogue.Count == 0)
        {
            Debug.LogWarning(
                "NPCDialogueUI: No intro dialogue found for Level " +
                levelData.levelIndex
            );

            FinishDialogue();
            return;
        }

        if (panel != null)
            panel.SetActive(true);

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (_currentDialogue == null ||
            _currentLineIndex < 0 ||
            _currentLineIndex >= _currentDialogue.Count)
        {
            FinishDialogue();
            return;
        }

        if (dialogueText != null)
            dialogueText.text = _currentDialogue[_currentLineIndex];

        // Change button text on the final line
        if (continueButton != null)
        {
            TMP_Text buttonText =
                continueButton.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                if (_currentLineIndex >= _currentDialogue.Count - 1)
                    buttonText.text = "Start";
                else
                    buttonText.text = "→";
            }
        }
    }

    private void HandleContinue()
    {
        _currentLineIndex++;

        if (_currentDialogue == null ||
            _currentLineIndex >= _currentDialogue.Count)
        {
            FinishDialogue();
            return;
        }

        ShowCurrentLine();
    }

    private void FinishDialogue()
    {
        if (panel != null)
            panel.SetActive(false);

        Action callback = _onDialogueFinished;

        _onDialogueFinished = null;
        _currentDialogue = null;
        _currentLineIndex = 0;

        callback?.Invoke();
    }

    /// <summary>
    /// Shows the completion dialogue one line at a time.
    /// </summary>
    public void ShowCompletion(LevelDataSO levelData, Action onFinished = null)
    {
        if (levelData == null)
        {
            Debug.LogError("NPCDialogueUI: LevelData is null.");
            return;
        }

        _onDialogueFinished = onFinished;

        if (npcNameText != null)
            npcNameText.text = levelData.npcName;

        if (npcPortraitImage != null)
            npcPortraitImage.sprite = levelData.npcPortrait;

        _currentDialogue = levelData.npcCompletionDialogue;
        _currentLineIndex = 0;

        if (_currentDialogue == null || _currentDialogue.Count == 0)
        {
            FinishDialogue();
            return;
        }

        if (panel != null)
            panel.SetActive(true);

        ShowCurrentLine();
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(HandleContinue);
    }
}