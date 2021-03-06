using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    [Header("Player Properties")]
    [SerializeField] int startPlayerLives = 3;

    [Header("Game Behaviour")]
    [SerializeField] float reloadLevelTimeDelay = 1.5f;
    [SerializeField] float loadNextLevelTimeDelay = 2f;

    // state
    int playerLives = 3;
    int numCoins = 0;
    int enemiesDefeated = 0;
    int batsDefeated = 0;
    float timeElapsed = 0f;
    bool timer = true;
    int currentSceneIndex = 0;

    // singleton
    private static GameSession _instance;
    public static GameSession instance {
        get {
            AppIntegrity.AssertPresent<GameSession>(_instance);
            return _instance;
        }
    }

    public int PlayerLives => playerLives;
    public int NumCoins => numCoins;
    public int EnemiesDefeated => enemiesDefeated;
    public int BatsDefeated => batsDefeated;
    public float TimeElapsed => timeElapsed;

    void Awake()
    {
        // // ALTERNATIVE SINGLETON PATTERN
        // int numGameSessionsAlreadyExist = FindObjectsOfType<GameSession>().Length;
        // if (numGameSessionsAlreadyExist > 1)
        // {
        //     Destroy(gameObject);
        // } else {
        //     DontDestroyOnLoad(gameObject);
        // }

        if (_instance != null) {
            if (_instance != this) { Destroy(this.gameObject); }
            return;
        }

        DontDestroyOnLoad(this.gameObject);
        _instance = this;
    }

    void Init()
    {
        playerLives = startPlayerLives;
        numCoins = 0;
        enemiesDefeated = 0;
        timer = true;
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    void Start()
    {
        Init();
        AudioManager.instance.StartMusic();
        RefreshUI();
    }

    void Update() {
        if (timer) {
            timeElapsed += Time.deltaTime;
        }
    }

    void RefreshUI()
    {
        PlayerUI.instance.SetLives(playerLives);
        PlayerUI.instance.SetNumCoins(numCoins);
    }

    public void StopGameTimer() {
        timer = false;
    }

    public void ProcessAcquireCoin()
    {
        numCoins += 1;
        AudioManager.instance.Play("Coin");

        if (numCoins % 100 == 0) {
            playerLives += 1;
            AudioManager.instance.Play("OneUp");
        }

        RefreshUI();
    }

    public void ProcessEnemyDeath()
    {
        enemiesDefeated += 1;
    }

    public void ProcessBatDeath()
    {
        batsDefeated += 1;
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
    public void ProcessLevelComplete(string sceneName)
    {
        StartCoroutine(LoadSpecificLevel(sceneName));
    }

    IEnumerator ReloadLevel()
    {
        yield return new WaitForSecondsRealtime(reloadLevelTimeDelay);

        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        AppIntegrity.AssertPresent<PlayerMovement>(playerMovement);
        playerMovement.Respawn();
        RefreshUI();
    }

    IEnumerator LoadSpecificLevel(string sceneName)
    {
        yield return new WaitForSecondsRealtime(loadNextLevelTimeDelay);

        AudioManager.instance.Stop("PlayerFootsteps");
        SceneManager.LoadScene(sceneName);
        yield return null;
    }

    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSecondsRealtime(loadNextLevelTimeDelay);

        int nextSceneIndex = currentSceneIndex + 1;
        if (nextSceneIndex == SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 0;
        }

        currentSceneIndex = nextSceneIndex;

        AudioManager.instance.Stop("PlayerFootsteps");
        SceneManager.LoadScene(nextSceneIndex);
        yield return null;
    }

    IEnumerator GameOver()
    {
        yield return new WaitForSecondsRealtime(reloadLevelTimeDelay);

        AudioManager.instance.Stop("PlayerFootsteps");
        SceneManager.LoadScene("GameOverScreen");
    }
}
