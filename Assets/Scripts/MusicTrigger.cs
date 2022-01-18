using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    [SerializeField] string musicTrack;

    [SerializeField] bool enable = true;

    [SerializeField] bool fade = true;

    void Start() {
        AppIntegrity.AssertNonEmptyString(musicTrack);   
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag != "Player") return;

        if (musicTrack == "*")
        {
            AudioManager.instance.StartMusic();
        }
        else
        {
            if (enable)
            {
                if (fade)
                {
                    AudioManager.instance.FadeInTrackByName(musicTrack);
                }
                else
                {
                    AudioManager.instance.EnableTrackByName(musicTrack);
                }
            }
            else
            {
                if (fade)
                {
                    AudioManager.instance.FadeOutTrackByName(musicTrack);
                }
                else
                {
                    AudioManager.instance.DisableTrackByName(musicTrack);
                }
            }
        }

        Destroy(gameObject);
        
    }
}
