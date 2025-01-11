using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneAudioData : MonoBehaviour
{
    public AudioClip BGMClip;
    public AudioClip BGMAmbienceClip;

    public void Awake()
    {
        SceneManager.sceneLoaded += SetupBGM;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SetupBGM;
    }

    void SetupBGM(Scene scene, LoadSceneMode mode)
    {
        //it's the same audio clip, we do not set it to avoid restarting the soundtrack 
        if(AudioManager.Instance.BGMSource.clip == BGMClip)
            return;
        
        AudioManager.Instance.SetBGMClip(BGMClip, BGMAmbienceClip);
    }
}
