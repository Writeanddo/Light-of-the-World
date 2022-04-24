using UnityEngine;
using UnityEngine.InputSystem;

public class LevelController : MonoBehaviour
{
    [SerializeField]
    GameObject pauseMenu;

    bool isMenuOpened;

    private void Awake() => CloseMenu(false);
    private void Start() => GameManager.instance.FadeIntoLevel();

    /// <summary>
    /// TODO: prevent this from happening while the game manager is actively doing things
    /// </summary>
    private void LateUpdate()
    {
        if (GameManager.instance.DisableUI)
            return;

        // Unless we are on WebGL we want to check for either ESC or GamePad to pause the game
        if(Application.platform != RuntimePlatform.WebGLPlayer)
        {
            if(Gamepad.current.startButton.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame)
                ToggleMenu();

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                Application.Quit();
        } 
        else
        {
            // For webgl we will check ENTER
            if (Gamepad.current.startButton.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame || 
                Keyboard.current.numpadEnterKey.wasPressedThisFrame)
                ToggleMenu();
        }
    }

    void ToggleMenu()
    {
        isMenuOpened = !isMenuOpened;
        if (isMenuOpened)
            OpenMenu();
        else
            CloseMenu();
    }

    void OpenMenu()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        GameManager.instance.PauseGame();
    }

    void CloseMenu(bool resetTimeScale = true)
    {
        pauseMenu.SetActive(false);
        if(resetTimeScale)
            Time.timeScale = 1f;
        GameManager.instance.ResumeGame();        
    }

    public void OnResumeButtonPressed() => CloseMenu();

    public void OnRetryButtonPressed()
    {
        CloseMenu(false);
        GameManager.instance.RestartFromLastCheckpoint();
    }
}
