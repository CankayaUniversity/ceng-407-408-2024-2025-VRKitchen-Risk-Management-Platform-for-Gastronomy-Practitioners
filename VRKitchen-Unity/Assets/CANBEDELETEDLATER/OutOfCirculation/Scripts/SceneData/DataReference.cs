using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class DataReference : MonoBehaviour
{
    public static DataReference Instance { protected set; get; }
    
    public InputActionAsset DefaultInputActionAsset;

    public NavMeshAgentController PlayerReference { get; private set; }

    public AudioMixer Mixer;

    public TMP_FontAsset RegularFont;
    public TMP_FontAsset OpenDislexicFont;

    [SerializeField]
    private NavMeshAgentController m_PlayerPrefab;
    [SerializeField]
    private GameObject m_SceneSetuPrefab;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Load()
    {
        Instance = Resources.Load<DataReference>("DataReference");
    }

    public void CreateUI()
    {
        var ui = Instantiate(m_SceneSetuPrefab);
        DontDestroyOnLoad(ui);
    }

    public void SpawnGameData()
    {
        PlayerReference = Instantiate(m_PlayerPrefab);
        PlayerReference.gameObject.SetActive(false);
        DontDestroyOnLoad(PlayerReference.gameObject);
    }
}
