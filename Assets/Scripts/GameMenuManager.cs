using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuManager : MonoBehaviour
{
    void Start() {
        AudioManager.instance.StopMusic();
        AudioManager.instance.PlayTrackByName("Drums");
        
        GameSession session = FindObjectOfType<GameSession>();
        if (session != null) Destroy(session.gameObject);
    }

    // PUBLIC METHODS

    public void PlayGame() {
        SceneManager.LoadScene("Level10");
    }

    public void QuitGame() {
        Debug.Log("QUITTING GAME!");
        Application.Quit();
    }
}
