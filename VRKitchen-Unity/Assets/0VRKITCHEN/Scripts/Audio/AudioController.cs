using System.Collections;
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
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip placingMeatSound;
    [SerializeField] private AudioClip placingChickenSound;
    [SerializeField] private AudioClip fryingSound;
    [SerializeField] private AudioClip cuttingSound;
    [SerializeField] private AudioClip boilingSound;
    [SerializeField] private AudioClip placingPlateSound;
    [SerializeField] private AudioClip closingPanSound;
    [SerializeField] private AudioClip choppingMeatSound;
    [SerializeField] private AudioClip fireExtinguisherSound;
    [SerializeField] private AudioClip exclamationSound;
    [SerializeField] private AudioClip kitchenTimer;


    [Header("Volume Settings")]
    [SerializeField] private float fireSoundVolume = 0.5f;


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
    public void PlayWalkingSound()
    {
        PlaySound(walkingSound);
    }

    public void PlayExclamationSound()
    {
        PlaySound(exclamationSound);
    }


    public void PlayFireSound()
    {
        AudioSource source = GetOrCreateAudioSource();
        if (source != null && fireSound != null)
        {
            source.clip = fireSound;
            source.loop = true;
            source.volume = fireSoundVolume;
            source.Play();
        }
    }


    public AudioClip GetFireClip()
    {
        return fireSound;
    }


    public void PlayCuttingSound()
    {
        PlaySound(cuttingSound);
    }

    public void PlayPlacingMeatSound()
    {
        PlaySound(placingMeatSound);
    }

    public void PlayPlacingChickenSound()
    {
        PlaySound(placingChickenSound);
    }

    public void PlayBoilingSound()
    {
        PlaySound(boilingSound);
    }

    public void PlayFryingSound()
    {
        PlaySound(fryingSound);
    }

    public void PlayPlacingPlateSound()
    {
        PlaySound(placingPlateSound);
    }

    public void PlayClosingPanSound()
    {
        PlaySound(closingPanSound);
    }

    public void PlayChoppingMeatSound()
    {
        PlaySound(choppingMeatSound);
    }

    public void PlayKitchenTimerSound()
    {
        PlaySound(kitchenTimer);
    }

    public void PlayFireExtinguisherSound()
    {
        PlaySound(fireExtinguisherSound);
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

    public AudioSource PlayLoopingSound(AudioClip clip, Vector3 position)
    {
        if (clip == null) return null;

        AudioSource source = GetOrCreateAudioSource();
        if (source != null)
        {
            source.transform.position = position;
            source.clip = clip;
            source.loop = true;
            source.Play();
        }
        return source;
    }

    public void StopLoopingSound(AudioSource source, float fadeDuration = 0.5f)
    {
        if (source != null)
        {
            StartCoroutine(FadeOutAndStop(source, fadeDuration));
        }
    }


    private IEnumerator FadeOutAndStop(AudioSource source, float duration)
    {
        if (source == null) yield break;

        float startVolume = source.volume;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            yield return null;
        }

        source.Stop();
        source.clip = null;
        source.loop = false;
        source.volume = startVolume; // Reset volume for future use
    }
}
