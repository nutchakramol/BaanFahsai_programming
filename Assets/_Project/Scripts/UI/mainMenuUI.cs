using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scene Name")]
    public string levelSelectScene = "LevelSelect";

    // ปุ่ม Start
    public void StartGame()
    {
        SceneManager.LoadScene(levelSelectScene);
    }

    // ปุ่ม Continue
    public void ContinueGame()
    {
        Debug.Log("Continue Game");
        // ต่อกับระบบ Save ของ Spy ภายหลัง
    }

    // ปุ่ม Settings
    public void OpenSettings(GameObject settingsPanel)
    {
        settingsPanel.SetActive(true);
    }

    // ปุ่ม Close Settings
    public void CloseSettings(GameObject settingsPanel)
    {
        settingsPanel.SetActive(false);
    }

    // ปุ่ม Exit
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