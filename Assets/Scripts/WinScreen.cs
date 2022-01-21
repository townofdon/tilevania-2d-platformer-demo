using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WinScreen : MonoBehaviour
{
    // [SerializeField] TextMesh
    [SerializeField] TextMeshProUGUI coins;
    [SerializeField] TextMeshProUGUI enemies;
    [SerializeField] TextMeshProUGUI timeElapsed;

    // 150 bpm - see: https://tuneform.com/tools/time-tempo-bpm-to-milliseconds-ms
    const float OneBar = 1.6f;

    void Start() {
        AppIntegrity.AssertPresent<TextMeshProUGUI>(coins);
        AppIntegrity.AssertPresent<TextMeshProUGUI>(enemies);
        AppIntegrity.AssertPresent<TextMeshProUGUI>(timeElapsed);

        WinMusic();

        PlayerUI.Remove();
        PauseMenu.Remove();
        GameSession session = FindObjectOfType<GameSession>();

        if (session != null) {
            session.StopGameTimer();
            coins.text = session.NumCoins.ToString();
            enemies.text = session.EnemiesDefeated.ToString();
            timeElapsed.text = Utils.ToTimeString(session.TimeElapsed);
        } else {
            coins.text = "42";
            enemies.text = "64";
            timeElapsed.text = Utils.ToTimeString(155f);
        }

    }

    void WinMusic() {
        AudioManager.instance.StopMusic();
        AudioManager.instance.PlayTrackByName("SuxessStrings");
        AudioManager.instance.PlayTrackByName("SuxessBass");
        AudioManager.instance.PlayTrackByName("SuxessDrums");
        AudioManager.instance.DisableTrackByName("SuxessDrums");
        StartCoroutine(PlayDrums());
    }

    public void ReturnToMainMenu() {
        StopAllCoroutines();
        SceneManager.LoadScene("GameStart");
    }

    IEnumerator PlayDrums() {
        yield return new WaitForSecondsRealtime(OneBar * 8f);
        AudioManager.instance.EnableTrackByName("SuxessDrums");
    }
}
