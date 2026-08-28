using UnityEngine;

public class NPCDialogueBootstrapper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelDataSO levelData;
    [SerializeField] private NPCDialogueUI npcDialogueUI;

    private void Start()
    {
        if (levelData == null)
        {
            Debug.LogError("NPCDialogueBootstrapper: LevelData is not assigned.");
            return;
        }

        if (npcDialogueUI == null)
        {
            Debug.LogError("NPCDialogueBootstrapper: NPCDialogueUI is not assigned.");
            return;
        }

        npcDialogueUI.Show(levelData, OnDialogueFinished);
    }

    private void OnDialogueFinished()
    {
        Debug.Log("NPC dialogue finished. Player can start decorating.");
    }
}