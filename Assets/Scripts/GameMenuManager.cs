using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMenuManager : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject controlsMenu;
    [SerializeField] Button startButton;
    [SerializeField] Button backButton;

    bool everFocused = false;

    void Start() {
        AppIntegrity.AssertPresent<GameObject>(mainMenu);
        AppIntegrity.AssertPresent<GameObject>(controlsMenu);
        AppIntegrity.AssertPresent<GameObject>(startButton);
        AppIntegrity.AssertPresent<GameObject>(backButton);
        mainMenu.SetActive(true);
        controlsMenu.SetActive(false);
        
        AudioManager.instance.StopMusic();
        AudioManager.instance.PlayTrackByName("Drums");

        GameSession session = FindObjectOfType<GameSession>();
        if (session != null) Destroy(session.gameObject);

        PauseMenu.Remove();
        PlayerUI.Remove();
    }

    // PUBLIC METHODS

    public void PlayGame() {
        AudioManager.instance.Play("MenuSelect");
        SceneManager.LoadScene("Level10");
    }

    public void QuitGame() {
        AudioManager.instance.Play("MenuSecondary");
        Debug.Log("QUITTING GAME!");
        Application.Quit();
    }

    public void ShowControlsMenu() {
        AudioManager.instance.Play("MenuSecondary");
        mainMenu.SetActive(false);
        controlsMenu.SetActive(true);
        backButton.Select();
    }

    public void ShowMainMenu() {
        AudioManager.instance.Play("MenuSecondary");
        controlsMenu.SetActive(false);
        mainMenu.SetActive(true);
        startButton.Select();
    }

    public void OnButtonFocusSound()
    {
        if (everFocused) AudioManager.instance.Play("MenuFocus");
        everFocused = true;
    }
}
