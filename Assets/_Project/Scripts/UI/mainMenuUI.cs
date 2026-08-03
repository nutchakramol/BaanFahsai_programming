using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scene Name")]
    public string levelSelectScene = "LevelSelect";

    public void StartGame()
    {
        SceneManager.LoadScene(levelSelectScene);
    }

    public void ContinueGame()
    {
        Debug.Log("Continue Game");
    }

    // ปุ่ม Settings
    public void OpenSettings(GameObject settingsPanel)
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings(GameObject settingsPanel)
    {
        settingsPanel.SetActive(false);
    }

    public void ExitGame()
    {
        Debug.Log("Exit");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}