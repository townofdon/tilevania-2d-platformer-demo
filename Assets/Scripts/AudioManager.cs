using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoSingleton<AudioManager>
{
    [SerializeField] AudioMixerGroup musicMix;
    [SerializeField] AudioMixerGroup soundFXMix;

    [SerializeField] Sound[] sounds;
    [SerializeField] Sound[] musicTracks;

    [Tooltip("Sound that should be played when a sound is not found")]
    [SerializeField] Sound nullSound;

    [SerializeField] float musicTrackFadeInTime = 0.75f;
    [SerializeField] float musicTrackFadeOutTime = 0.15f;

    void Awake()
    {
        base.Awake();

        // set up audio sources for all sounds
        foreach (Sound s in sounds)
        {
            s.setSource(gameObject.AddComponent<AudioSource>(), soundFXMix);
        }

        // set up audio sources for all music tracks
        foreach (Sound s in musicTracks)
        {
            s.setSource(gameObject.AddComponent<AudioSource>(), musicMix);
        }

        // set up audio source for null sound
        nullSound.setSource(gameObject.AddComponent<AudioSource>(), soundFXMix);
    }

    Sound FindSound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) {
            Debug.LogWarning("AudioManager: Sound \"" + name + "\" not found");
            return nullSound;
        }
        return s;
    }

    Sound FindMusicTrack(string name)
    {
        Sound s = Array.Find(musicTracks, track => track.name == name);
        if (s == null) {
            Debug.LogWarning("AudioManager: Music Track \"" + name + "\" not found");
            return nullSound;
        }
        return s;
    }

    public void Play(string name)
    {
        Sound s = FindSound(name);
        s.Play();
    }

    public void PlayAtLocation(string name, Vector3 location)
    {
        Sound s = FindSound(name);
        s.PlayAtLocation(location);
    }

    public void Stop(string name)
    {
        Sound s = FindSound(name);
        s.Stop();
    }

    public void StartMusic()
    {
        for (int i = 0; i < musicTracks.Length; i++)
        {
            // stop any track that is currently playing so that track times line up correctly
            musicTracks[i].Stop();

            // start playing silently so that tracks are playing in sync;
            // we can simply raise the volume on a track to have it start
            // playing at the appropriate time.
            musicTracks[i].SilenceTrack();

            if (i == 0) musicTracks[i].UnsilenceTrack();

            musicTracks[i].PlayTrack();
        }
    }

    public void StopMusic()
    {
        for (int i = 0; i < musicTracks.Length; i++)
        {
            musicTracks[i].Stop();
        }
    }

    public void EnableTrackByName(string name)
    {
        Sound musicTrack = FindMusicTrack(name);
        musicTrack.UnsilenceTrack();
    }

    public void DisableTrackByName(string name)
    {
        Sound musicTrack = FindMusicTrack(name);
        musicTrack.SilenceTrack();
    }

    public void PlayTrackByName(string name)
    {
        Sound musicTrack = FindMusicTrack(name);
        musicTrack.UnsilenceTrack();
        musicTrack.PlayTrack();
    }

    public void FadeInTrackByName(string name)
    {
        Sound musicTrack = FindMusicTrack(name);
        StartCoroutine(musicTrack.FadeIn(musicTrackFadeInTime));
    }

    public void FadeOutTrackByName(string name)
    {
        Sound musicTrack = FindMusicTrack(name);
        StartCoroutine(musicTrack.FadeOut(musicTrackFadeOutTime));
    }

    public void PauseMusic()
    {
        foreach (Sound s in musicTracks)
        {
            s.PauseTrack();
        }
    }

    public void UnPauseMusic()
    {
        foreach (Sound s in musicTracks)
        {
            s.UnPauseTrack();
        }
    }
}
