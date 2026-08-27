using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
public class NPCDialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Image npcPortraitImage;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button continueButton;
    private Action _onContinue;
    private List<string> _dialogues;
    private int _currentIndex;
    private void Awake()
    {
        if (panel != null) panel.SetActive(false);
        if (continueButton != null) continueButton.onClick.AddListener(HandleContinue);
    }
    public void Show(LevelDataSO levelData, Action onContinue)
    {
        _onContinue = onContinue;
        _dialogues = levelData.npcIntroDialogue;
        _currentIndex = 0;
        if (npcNameText != null) npcNameText.text = levelData.npcName;
        if (npcPortraitImage != null) npcPortraitImage.sprite = levelData.npcPortrait;
        if (panel != null) panel.SetActive(true);
        ShowCurrentDialogue();
    }
    private void ShowCurrentDialogue()
    {
        if (_dialogues == null || _dialogues.Count == 0)
        {
            FinishDialogue();
            return;
        }
        if (dialogueText != null) dialogueText.text = _dialogues[_currentIndex];
    }
    private void HandleContinue()
    {
        if (_dialogues == null || _dialogues.Count == 0)
        {
            FinishDialogue();
            return;
        }
        _currentIndex++;
        if (_currentIndex >= _dialogues.Count)
        {
            FinishDialogue();
            return;
        }
        ShowCurrentDialogue();
    }
    private void FinishDialogue()
    {
        if (panel != null) panel.SetActive(false);
        _onContinue?.Invoke();
        _onContinue = null;
        _dialogues = null;
        _currentIndex = 0;
    }
}