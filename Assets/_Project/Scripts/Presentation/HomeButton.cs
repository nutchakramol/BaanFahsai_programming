using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeButton : MonoBehaviour
{
    public void GoHome()
    {
        MainMenuUI.ReturnToLevelSelectOnLoad = true;
        SceneManager.LoadScene("SampleScene");
    }
}