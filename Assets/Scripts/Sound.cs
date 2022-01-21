using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    [SerializeField] string soundName;
    public string name => soundName;

    [SerializeField] AudioClip clip;
    public AudioClip Clip => clip;

    [HideInInspector]
    AudioSource source;

    [Range(0f, 1f)]
    [SerializeField] float volume = 0.7f;

    [Range(.1f, 3f)]
    [SerializeField] float pitch = 1f;

    [SerializeField] bool loop = false;

    [Range(0f, 0.5f)]
    [SerializeField] float volumeVarianceMultiplier = 0.1f;

    [Range(0f, 0.5f)]
    [SerializeField] float pitchVarianceMultiplier = 0.1f;

    // STATE
    float volumeFadeStart = 0f;
    float timeFade = 0f;

    float Randomize(float initialValue, float randomness = 0f)
    {
        if (randomness <= 0) return initialValue;

        return initialValue * (1 + UnityEngine.Random.Range(
            -randomness / 2f,
            randomness / 2f
        ));
    }

    void UpdateVariance()
    {
        source.volume = Mathf.Min(Randomize(volume, volumeVarianceMultiplier), 1f);
        source.pitch = Mathf.Min(Randomize(pitch, pitchVarianceMultiplier), 1f);
    }

    public void setSource(AudioSource _source, AudioMixerGroup mix)
    {
        source = _source;
        source.clip = clip;
        source.loop = loop;
        source.volume = volume;
        source.pitch = pitch;
        source.playOnAwake = false;
        source.outputAudioMixerGroup = mix;
        AppIntegrity.AssertNonEmptyString(soundName);
        AppIntegrity.AssertPresent<AudioClip>(clip);
    }

    public void Play()
    {
        UpdateVariance();
        if (loop)
        {
            if (source.isPlaying) return;
            source.Play();
        } else {
            source.PlayOneShot(clip);
        }
    }

    public void PlayAtLocation(Vector3 location)
    {
        UpdateVariance();
        AudioSource.PlayClipAtPoint(clip, location, 1f);
    }

    public void Stop()
    {
        source.Stop();
    }

    // MUSIC TRACK METHODS

    public IEnumerator FadeIn(float duration)
    {
        volumeFadeStart = source.volume;
        timeFade = 0f;
        yield return null;

        while (source.volume < volume || timeFade < duration)
        {
            // TODO: ADD EASING - currently is only a linear fadeout
            source.volume = Mathf.Lerp(volumeFadeStart, volume, timeFade / duration);
            timeFade += Time.deltaTime;
            yield return null;
        }

        yield return null;
    }

    public IEnumerator FadeOut(float duration)
    {
        volumeFadeStart = source.volume;
        timeFade = 0f;
        yield return null;

        while (source.volume > 0f || timeFade < duration)
        {
            // TODO: ADD EASING - currently is only a linear fadeout
            source.volume = Mathf.Lerp(volumeFadeStart, 0f, timeFade / duration);
            timeFade += Time.deltaTime;
            yield return null;
        }

        yield return null;
    }

    public void PlayTrack()
    {
        if (source.isPlaying) return;

        source.UnPause();
        source.Play();
    }

    public void PauseTrack()
    {
        source.Pause();
    }

    public void UnPauseTrack()
    {
        source.UnPause();
    }

    public void SilenceTrack()
    {
        source.volume = 0f;
    }

    public void UnsilenceTrack()
    {
        source.volume = volume;
    }
}
