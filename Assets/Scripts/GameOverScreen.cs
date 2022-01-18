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
        StartCoroutine(PlayDrums());
        StartCoroutine(PlaySquare());
        Destroy(FindObjectOfType<PlayerUI>().gameObject);
    }

    public void ReturnToMainMenu() {
        StopAllCoroutines();
        SceneManager.LoadScene("GameStart");
    }

    IEnumerator PlayDrums() {
        yield return new WaitForSecondsRealtime(OneBar * 8f);
        AudioManager.instance.PlayTrackByName("BawzDrums");
    }

    IEnumerator PlaySquare() {
        yield return new WaitForSecondsRealtime(OneBar * 16f);
        AudioManager.instance.PlayTrackByName("BawzSquare");
    }
}
