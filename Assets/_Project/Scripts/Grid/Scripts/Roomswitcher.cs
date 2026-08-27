using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomSwitcher : MonoBehaviour
{
    [Header("Scene Names — must match Build Settings exactly")]
    public string bedroomScene = "Bedroom";
    public string bathroomScene = "Toilet";
    public string livingRoomScene = "LivingRoom";
    public string kitchenScene = "Kitchen";
    public string atticScene = "Attic";

    public void GoToBedroom() => LoadRoom(bedroomScene);
    public void GoToBathroom() => LoadRoom(bathroomScene);
    public void GoToLivingRoom() => LoadRoom(livingRoomScene);
    public void GoToKitchen() => LoadRoom(kitchenScene);
    public void GoToAttic() => LoadRoom(atticScene);

    private void LoadRoom(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[RoomSwitcher] Scene name is empty.");
            return;
        }

        Debug.Log($"[RoomSwitcher] Loading {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}