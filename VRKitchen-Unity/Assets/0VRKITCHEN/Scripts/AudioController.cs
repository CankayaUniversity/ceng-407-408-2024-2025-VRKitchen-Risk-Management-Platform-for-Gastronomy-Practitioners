using System.Collections.Generic;
using UnityEngine;

public class AudioController : SingletonBehaviour<AudioController>
{
    [Header("AudioSourcePrefab")]
    [SerializeField] private AudioSource audioSourcePrefab;

    [Header("Audio Source")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private List<AudioSource> audioSources = new List<AudioSource>();

    [Header("Audio Clip")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip walkingSound;

    private void Start()
    {
        // We skip background music for now
        // musicSource.clip = backgroundMusic;
        // musicSource.loop = true;
        // musicSource.Play();
    }

    // Call this from any script to play a one-shot sound
    public void PlaySound(AudioClip clip)
    {
        AudioSource source = GetOrCreateAudioSource();
        if (source != null && clip != null)
        {
            source.PlayOneShot(clip);
        }
    }

    // Special method just for walking sound
    public void PlayWalkingSound()
    {
        PlaySound(walkingSound);
    }

    private AudioSource GetOrCreateAudioSource()
    {
        foreach (var source in audioSources)
        {
            if (!source.isPlaying)
                return source;
        }

        AudioSource newSource = Instantiate(audioSourcePrefab, transform);
        audioSources.Add(newSource);
        return newSource;
    }
}
