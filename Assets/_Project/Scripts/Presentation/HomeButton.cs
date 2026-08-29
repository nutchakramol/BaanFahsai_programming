using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeButton : MonoBehaviour
{
    public void GoHome()
    {
        SceneManager.LoadScene("SampleScene");
    }
}