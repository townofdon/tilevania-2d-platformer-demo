using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoSingleton<PauseMenu>
{
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] GameObject pauseMenuCanvas;
    [SerializeField] GameObject eventSystem;

    bool isPaused;

    public bool IsPaused => isPaused;

    void Start()
    {
        pauseMenuCanvas.SetActive(false);
        eventSystem.SetActive(false);
    }

    // public static void Remove() {

    //     if (instance != null) {
    //         instance.pauseMenuCanvas.SetActive(false);
    //         instance.eventSystem.SetActive(false);
    //         Destroy(instance.gameObject);
    //     }
    //     PauseMenu[] menus = FindObjectsOfType<PauseMenu>();
    //     foreach (PauseMenu menu in menus)
    //     {
    //         if (menu != null && menu.gameObject != null) {
    //             Destroy(menu.gameObject);
    //         }
    //     }
    // }

    public void TogglePause() {
        if (isPaused) {
            Unpause();
        } else {
            Pause();
        }
    }

    public void Pause() {
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenuCanvas.SetActive(true);
        eventSystem.SetActive(true);
        audioMixer.SetFloat("MusicVolume", -10f);
        AudioManager.instance.Play("MenuPause");
        AudioManager.instance.PauseMusic();
    }

    public void Unpause() {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuCanvas.SetActive(false);
        eventSystem.SetActive(false);
        audioMixer.SetFloat("MusicVolume", 0f);
        AudioManager.instance.UnPauseMusic();
    }

    public void QuitGame() {
        Unpause();
        SceneManager.LoadScene("GameStart");
    }
}
