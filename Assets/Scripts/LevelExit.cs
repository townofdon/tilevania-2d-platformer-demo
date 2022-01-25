using UnityEngine;

public class LevelExit : MonoBehaviour
{
    [SerializeField] string nextLevel = "";

    [Header("Colors")]
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
    bool levelCompleted = false;

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
        if (levelCompleted) return;

        if (other.gameObject.tag == "Player")
        {
            levelCompleted = true;
            particles.Play();
            spriteRenderer.color = colorOn;
            if (nextLevel == "") {
                FindObjectOfType<GameSession>().ProcessLevelComplete();
            } else {
                FindObjectOfType<GameSession>().ProcessLevelComplete(nextLevel);
            }

            AudioManager.instance.Play("LevelComplete");
        }
    }
}
