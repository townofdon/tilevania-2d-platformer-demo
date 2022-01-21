using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    // 150 bpm - see: https://tuneform.com/tools/time-tempo-bpm-to-milliseconds-ms
    const float OneBar = 1.6f;

    void Start() {
        AudioManager.instance.StopMusic();
        AudioManager.instance.PlayTrackByName("BawzStrings");
        AudioManager.instance.PlayTrackByName("BawzBass");
        AudioManager.instance.PlayTrackByName("BawzDrums");
        AudioManager.instance.PlayTrackByName("BawzSquare");
        AudioManager.instance.DisableTrackByName("BawzDrums");
        AudioManager.instance.DisableTrackByName("BawzSquare");
        StartCoroutine(PlayDrums());
        StartCoroutine(PlaySquare());
        PlayerUI.Remove();
        PauseMenu.Remove();
    }

    public void ReturnToMainMenu() {
        StopAllCoroutines();
        SceneManager.LoadScene("GameStart");
    }

    IEnumerator PlayDrums() {
        yield return new WaitForSecondsRealtime(OneBar * 8f);
        AudioManager.instance.EnableTrackByName("BawzDrums");
    }

    IEnumerator PlaySquare() {
        yield return new WaitForSecondsRealtime(OneBar * 16f);
        AudioManager.instance.EnableTrackByName("BawzSquare");
    }
}
