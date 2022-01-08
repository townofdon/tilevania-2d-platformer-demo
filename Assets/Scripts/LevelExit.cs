using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [SerializeField] Color colorOff = Color.red;
    [SerializeField] Color colorOn = Color.cyan;

    SpriteRenderer spriteRenderer;
    ParticleSystem particles;

    bool isIncreasingOpacity = false;
    float opacity = 1f;
    float opacityMax = 0.9f;
    float opacityMin = .3f;
    float t = 0f;
    float pulseDuration = 1f;

    void Start()
    {
        spriteRenderer = Utils.GetRequiredComponent<SpriteRenderer>(this.gameObject);
        particles = Utils.GetRequiredComponent<ParticleSystem>(this.gameObject);

        particles.Stop();

        spriteRenderer.color = colorOff;
    }

    void Update()
    {
        t += Time.deltaTime;

        Pulse();
    }

    void Pulse()
    {
        if (t >= pulseDuration)
        {
            isIncreasingOpacity = !isIncreasingOpacity;
            t = 0f;
        }

        opacity = isIncreasingOpacity
            ? Mathf.Lerp(opacityMin, opacityMax, t / pulseDuration)
            : Mathf.Lerp(opacityMax, opacityMin, t / pulseDuration);

        Color c = spriteRenderer.color;
        c.a = opacity;
        spriteRenderer.color = c;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player")
        {
            particles.Play();
            spriteRenderer.color = colorOn;
            StartCoroutine(LoadNextLevel());
        }
    }

    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSecondsRealtime(2f);

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex == SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 0;
        }

        SceneManager.LoadScene(nextSceneIndex);

        yield return null;
    }
}
