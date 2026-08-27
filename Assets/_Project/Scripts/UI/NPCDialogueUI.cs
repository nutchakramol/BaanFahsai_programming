using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class NPCDialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Image npcPortraitImage;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button continueButton;

    private Action _onContinue;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);
        if (continueButton != null) continueButton.onClick.AddListener(HandleContinue);
    }

    public void Show(LevelDataSO levelData, Action onContinue)
    {
        _onContinue = onContinue;

        if (npcNameText != null) npcNameText.text = levelData.npcName;
        if (dialogueText != null) dialogueText.text = levelData.npcIntroDialogue;
        if (npcPortraitImage != null) npcPortraitImage.sprite = levelData.npcPortrait;

        if (panel != null) panel.SetActive(true);
    }

    private void HandleContinue()
    {
        if (panel != null) panel.SetActive(false);
        _onContinue?.Invoke();
        _onContinue = null;
    }
}