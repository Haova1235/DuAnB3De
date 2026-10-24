using UnityEngine;
using UnityEngine.SceneManagement;

public class KeypadClose : MonoBehaviour
{
    public void CloseKeypad()
    {
        SceneManager.UnloadSceneAsync("KeypadSceneStandard");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}