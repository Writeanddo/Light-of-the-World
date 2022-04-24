using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    GameObject titleScreen;

    [SerializeField]
    GameObject controlsScreen;

    [SerializeField]
    GameObject settingsScreen;

    private void Awake()
    {
        titleScreen.SetActive(true);
        controlsScreen.SetActive(false);
        settingsScreen.SetActive(false);
    }

    public void OnPlayButtonPressed() 
    {
        titleScreen.SetActive(false);
        GameManager.instance.OnPlayButtonPressed();
    }

    public void OnControlsButtonPressed() 
    {
        titleScreen.SetActive(false);
        controlsScreen.SetActive(true);
    }

    public void OnSettingsButtonPressed() 
    {
        titleScreen.SetActive(false);
        settingsScreen.SetActive(true);
    }

    public void OnBackButtonPressed() 
    {
        titleScreen.SetActive(true);
        controlsScreen.SetActive(false);
        settingsScreen.SetActive(false);
    }

    private void Update()
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                Application.Quit();
        }
    }
}
