using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    [Header("Player Properties")]
    [SerializeField] int playerLives = 3;

    [Header("Game Behaviour")]
    [SerializeField] float reloadLevelTimeDelay = 1.5f;
    [SerializeField] float loadNextLevelTimeDelay = 2f;

    public int PlayerLives => playerLives;

    void Awake()
    {
        int numGameSessionsAlreadyExist = FindObjectsOfType<GameSession>().Length;

        if (numGameSessionsAlreadyExist > 1)
        {
            Destroy(gameObject);
        } else {
            DontDestroyOnLoad(gameObject);
        }
    }

    void Update()
    {
        
    }

    public void ProcessPlayerDeath()
    {
        playerLives -= 1;

        if (playerLives >= 1)
        {
            StartCoroutine(ReloadLevel());
        } else {

            StartCoroutine(GameOver());
        }
    }

    public void ProcessLevelComplete()
    {
        StartCoroutine(LoadNextLevel());
    }

    IEnumerator ReloadLevel()
    {
        yield return new WaitForSecondsRealtime(reloadLevelTimeDelay);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSecondsRealtime(loadNextLevelTimeDelay);

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex == SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 0;
        }

        SceneManager.LoadScene(nextSceneIndex);
        yield return null;
    }

    IEnumerator GameOver()
    {
        yield return new WaitForSecondsRealtime(reloadLevelTimeDelay);

        SceneManager.LoadScene(0);
        Destroy(gameObject);
    }
}
